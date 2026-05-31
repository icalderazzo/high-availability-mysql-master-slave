-- Monthly Sales Report Stored Procedure
-- This will be replicated to the slave and executed there via ProxySQL

USE classicmodels;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetMonthlySalesReport$$

CREATE PROCEDURE GetMonthlySalesReport(
    IN report_year INT,
    IN report_month INT
)
BEGIN
    -- Monthly sales summary
    SELECT 
        DATE_FORMAT(o.orderDate, '%Y-%m') AS month,
        COUNT(DISTINCT o.orderNumber) AS total_orders,
        COUNT(DISTINCT o.customerNumber) AS unique_customers,
        SUM(od.quantityOrdered * od.priceEach) AS total_revenue,
        AVG(od.quantityOrdered * od.priceEach) AS avg_order_value,
        SUM(od.quantityOrdered) AS total_items_sold
    FROM orders o
    INNER JOIN orderdetails od ON o.orderNumber = od.orderNumber
    WHERE o.status IN ('Shipped', 'Resolved')
       AND YEAR(o.orderDate) = report_year
       AND MONTH(o.orderDate) = report_month
    GROUP BY DATE_FORMAT(o.orderDate, '%Y-%m');
    
    -- Top 10 products by revenue
    SELECT 
        p.productCode,
        p.productName,
        p.productLine,
        SUM(od.quantityOrdered) AS units_sold,
        SUM(od.quantityOrdered * od.priceEach) AS revenue
    FROM orders o
    INNER JOIN orderdetails od ON o.orderNumber = od.orderNumber
    INNER JOIN products p ON od.productCode = p.productCode
    WHERE o.status IN ('Shipped', 'Resolved')
      AND YEAR(o.orderDate) = report_year
      AND MONTH(o.orderDate) = report_month
    GROUP BY p.productCode, p.productName, p.productLine
    ORDER BY revenue DESC
    LIMIT 10;

END$$

DELIMITER ;