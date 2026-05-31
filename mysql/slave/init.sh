#!/bin/bash
# Slave initialization script
# On FIRST startup (empty data dir):
#   1. Starts MySQL in the background (docker-entrypoint.sh handles data dir init)
#   2. Waits for local MySQL and the master to be ready
#   3. Seeds the slave via mysqldump --master-data=2 from master
#      (--master-data=2 embeds CHANGE MASTER TO as a comment so we can parse the position)
#   4. Applies CHANGE MASTER TO with the parsed position + credentials, then START SLAVE
#   5. Stops the background MySQL and re-execs it in the foreground
#
# On SUBSEQUENT startups (data dir already initialized):
#   - Skips seeding and just starts MySQL normally (replication resumes automatically)

set -e

MASTER_HOST="${MASTER_HOST:-mysql-master}"
MASTER_PORT="${MASTER_PORT:-3306}"
MASTER_USER="${MASTER_USER:-replication_user}"
MASTER_PASSWORD="${MASTER_PASSWORD:-replication_password_123}"
MYSQL_ROOT_PASSWORD="${MYSQL_ROOT_PASSWORD:-your_super_secure_root_password}"

DATADIR="/var/lib/mysql"
INIT_SENTINEL="${DATADIR}/.slave_initialized"

_mysql_slave() {
  mysql -uroot -p"${MYSQL_ROOT_PASSWORD}" "$@"
}

if [ -f "${INIT_SENTINEL}" ]; then
  echo "[slave/init.sh] Data directory already initialized. Starting MySQL normally (replication will resume)..."
  exec docker-entrypoint.sh mysqld \
    --server-id=2 \
    --log-bin=mysql-bin \
    --binlog-format=row \
    --relay-log=mysql-relay-bin \
    --read-only=1
fi

echo "[slave/init.sh] First startup detected. Running full initialization..."

echo "[slave/init.sh] Starting MySQL in the background..."
docker-entrypoint.sh mysqld \
  --server-id=2 \
  --log-bin=mysql-bin \
  --binlog-format=row \
  --relay-log=mysql-relay-bin \
  --read-only=1 &

MYSQL_BG_PID=$!

echo "[slave/init.sh] Waiting for local MySQL to be ready..."
until _mysql_slave --silent -e "SELECT 1" 2>/dev/null; do
  if ! kill -0 "${MYSQL_BG_PID}" 2>/dev/null; then
    echo "[slave/init.sh] ERROR: Background MySQL process died unexpectedly."
    exit 1
  fi
  echo "[slave/init.sh] Local MySQL not ready yet, retrying in 2s..."
  sleep 2
done
echo "[slave/init.sh] Local MySQL is ready."

echo "[slave/init.sh] Waiting for master (${MASTER_HOST}:${MASTER_PORT}) to be ready..."
until mysqladmin ping -h"${MASTER_HOST}" -P"${MASTER_PORT}" -u"${MASTER_USER}" -p"${MASTER_PASSWORD}" --silent 2>/dev/null; do
  echo "[slave/init.sh] Master not ready yet, retrying in 2s..."
  sleep 2
done
echo "[slave/init.sh] Master is ready."

echo "[slave/init.sh] Dumping application databases from master (--master-data=2)..."
# We dump only application databases (NOT mysql/sys/information_schema/performance_schema)
# to avoid conflicts with system users that already exist on the slave.
# --master-data=2 embeds the binlog position as a SQL comment so we can parse it,
# without auto-executing a CHANGE MASTER TO that would conflict with our explicit one below.
DUMP_FILE="/tmp/master_dump.sql"
mysqldump \
  -h"${MASTER_HOST}" \
  -P"${MASTER_PORT}" \
  -uroot \
  -p"${MYSQL_ROOT_PASSWORD}" \
  --databases classicmodels \
  --master-data=2 \
  --single-transaction \
  --flush-logs \
  --triggers \
  --routines \
  --events \
  > "${DUMP_FILE}"

# Parse the binlog position from the dump comment:
# The line looks like: -- CHANGE MASTER TO MASTER_LOG_FILE='mysql-bin.000003', MASTER_LOG_POS=157;
MASTER_LOG_FILE=$(grep -m1 "CHANGE MASTER TO" "${DUMP_FILE}" | sed "s/.*MASTER_LOG_FILE='\\([^']*\\)'.*/\\1/")
MASTER_LOG_POS=$(grep -m1 "CHANGE MASTER TO" "${DUMP_FILE}" | sed "s/.*MASTER_LOG_POS=\\([0-9]*\\).*/\\1/")

echo "[slave/init.sh] Binlog position from dump: FILE=${MASTER_LOG_FILE}, POS=${MASTER_LOG_POS}"

echo "[slave/init.sh] Importing dump into slave..."
_mysql_slave < "${DUMP_FILE}"
rm -f "${DUMP_FILE}"

echo "[slave/init.sh] Creating application users on slave..."
_mysql_slave < /slave-init/init.sql

echo "[slave/init.sh] Configuring replication..."
_mysql_slave <<-EOSQL
  STOP SLAVE;
  CHANGE MASTER TO
    MASTER_HOST='${MASTER_HOST}',
    MASTER_PORT=${MASTER_PORT},
    MASTER_USER='${MASTER_USER}',
    MASTER_PASSWORD='${MASTER_PASSWORD}',
    MASTER_LOG_FILE='${MASTER_LOG_FILE}',
    MASTER_LOG_POS=${MASTER_LOG_POS};
  START SLAVE;
EOSQL

echo "[slave/init.sh] Replication started. Current slave status:"
_mysql_slave -e "SHOW SLAVE STATUS\G" \
  | grep -E "(Slave_IO_Running|Slave_SQL_Running|Last_Error|Master_Log_File|Read_Master_Log_Pos|Exec_Master_Log_Pos)"

# Mark initialization as complete so subsequent restarts skip this block
touch "${INIT_SENTINEL}"

echo "[slave/init.sh] Initialization complete. Stopping background MySQL and restarting in foreground..."
mysqladmin -uroot -p"${MYSQL_ROOT_PASSWORD}" shutdown 2>/dev/null || true
wait "${MYSQL_BG_PID}" 2>/dev/null || true

echo "[slave/init.sh] Restarting MySQL in foreground..."
exec docker-entrypoint.sh mysqld \
  --server-id=2 \
  --log-bin=mysql-bin \
  --binlog-format=row \
  --relay-log=mysql-relay-bin \
  --read-only=1
