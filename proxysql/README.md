# ProxySQL Configuration

## Overview

ProxySQL is configured to route database queries between MySQL master and slave:
- **CALL statements** → Slave (for reporting/read-heavy stored procedures)
- **All other queries** → Master (SELECT, INSERT, UPDATE, DELETE, DDL, etc.)

## Configuration Files

- `proxysql.cnf`: Main ProxySQL configuration with routing rules

## Routing Rules

| Rule | Pattern | Destination | Purpose |
|------|---------|-------------|---------|
| 10   | `^CALL` | Slave (Hostgroup 1) | Route stored procedures to slave |
| 100  | `.*`    | Master (Hostgroup 0) | Default: everything else to master |

## Connection Details

- **MySQL Interface**: Port 6033 (for application connections)
- **Admin Interface**: Port 6032 (for management and monitoring)
- **Username**: ordersapp
- **Password**: ordersapp_password
- **Default Schema**: classicmodels

## Monitoring

### Connect to Admin Interface

```bash
mysql -h127.0.0.1 -P6032 -uadmin -padmin
```

### View Query Routing Statistics

```sql
SELECT rule_id, hits, hostgroup AS destination 
FROM stats_mysql_query_rules 
ORDER BY rule_id;
```

### View Backend Server Status

```sql
SELECT hostgroup, srv_host, srv_port, status, ConnUsed, ConnFree, Queries 
FROM stats_mysql_connection_pool;
```

### View Query Digest

```sql
SELECT hostgroup, digest_text, count_star, sum_time/1000000 AS sum_time_sec
FROM stats_mysql_query_digest 
ORDER BY sum_time DESC 
LIMIT 20;
```

## Testing

### Test CALL Routing

```bash
# Execute a CALL statement
mysql -h127.0.0.1 -P6033 -uordersapp -pordersapp_password classicmodels \
  -e "CALL GetMonthlySalesReport(2024, 12);"

# Verify it went to slave (hostgroup 1)
mysql -h127.0.0.1 -P6032 -uadmin -padmin \
  -e "SELECT hostgroup, digest_text FROM stats_mysql_query_digest WHERE digest_text LIKE 'CALL%';"
```

### Test SELECT Routing

```bash
# Execute a SELECT statement
mysql -h127.0.0.1 -P6033 -uordersapp -pordersapp_password classicmodels \
  -e "SELECT COUNT(*) FROM orders;"

# Verify it went to master (hostgroup 0)
mysql -h127.0.0.1 -P6032 -uadmin -padmin \
  -e "SELECT hostgroup, digest_text FROM stats_mysql_query_digest WHERE digest_text LIKE 'SELECT COUNT%';"
```

## Troubleshooting

### Check ProxySQL Logs

```bash
docker logs proxysql
```

### Verify Backend Connections

```sql
-- Connect to admin interface
mysql -h127.0.0.1 -P6032 -uadmin -padmin

-- Check server status
SELECT * FROM mysql_servers;

-- Check connection pool
SELECT * FROM stats_mysql_connection_pool;
```

### Test Backend Connectivity

```bash
# Test master connection
docker exec -it proxysql mysql -hmysql-master -P3306 -uroot -pyour_super_secure_root_password -e "SELECT 1;"

# Test slave connection
docker exec -it proxysql mysql -hmysql-slave -P3306 -uroot -pyour_super_secure_root_password -e "SELECT 1;"
```

## Security Notes

- Change the default admin password (`admin:admin`) in production
- Restrict admin interface access (port 6032) to trusted IPs only
- Use strong passwords for the `ordersapp` user
- Monitor ProxySQL logs regularly for suspicious activity
