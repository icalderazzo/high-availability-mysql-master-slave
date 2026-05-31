-- Grant replication privileges to the replication user
-- This runs automatically after the classicmodels seed on first startup
ALTER USER 'replication_user'@'%' IDENTIFIED WITH 'mysql_native_password' BY 'replication_password_123';
GRANT REPLICATION SLAVE ON *.* TO 'replication_user'@'%';
FLUSH PRIVILEGES;

-- Create ordersapp user for ProxySQL connections
CREATE USER IF NOT EXISTS 'ordersapp'@'%' IDENTIFIED WITH 'mysql_native_password' BY 'ordersapp_password';
-- Grant permissions on classicmodels database
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON classicmodels.* TO 'ordersapp'@'%';
FLUSH PRIVILEGES;