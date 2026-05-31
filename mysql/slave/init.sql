-- Slave initialization SQL script
-- This script creates the ordersapp user on the slave database
-- It runs after the slave has been seeded from the master

-- Create ordersapp user for ProxySQL connections
CREATE USER IF NOT EXISTS 'ordersapp'@'%' IDENTIFIED WITH 'mysql_native_password' BY 'ordersapp_password';

-- Grant permissions on classicmodels database
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON classicmodels.* TO 'ordersapp'@'%';

FLUSH PRIVILEGES;
