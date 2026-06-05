--Section 1: Query Writing
--1. Top 3 employees by total net salary in 2024

SELECT TOP 3 
    e.name,
    e.department,
    SUM(p.net_salary) AS total_net_salary
FROM Employees e
JOIN Payroll p ON e.employee_id = p.employee_id
WHERE YEAR(p.pay_date) = 2024
GROUP BY e.name, e.department
ORDER BY total_net_salary DESC;

--2. Department totals

SELECT 
    e.department,
    SUM(p.gross_salary) AS total_gross_salary,
    SUM(p.tax_amount) AS total_tax_deducted,
    AVG(p.net_salary) AS avg_net_salary
FROM Employees e
JOIN Payroll p ON e.employee_id = p.employee_id
WHERE YEAR(p.pay_date) = 2024
GROUP BY e.department;


--Section 2: Joins & Subqueries
--3. Employees with no payroll in 2024

SELECT e.employee_id, e.name, e.department
FROM Employees e
WHERE NOT EXISTS (
    SELECT 1 
    FROM Payroll p 
    WHERE p.employee_id = e.employee_id 
      AND YEAR(p.pay_date) = 2024
);


--4. Most recent pay date & net salary per employee

SELECT e.name, e.department, p.pay_date, p.net_salary
FROM Employees e
JOIN Payroll p ON e.employee_id = p.employee_id
WHERE p.pay_date = (
    SELECT MAX(p2.pay_date)
    FROM Payroll p2
    WHERE p2.employee_id = e.employee_id
);


--Section 3: Performance & Optimization
--5. Indexing recommendation

--Index on pay_date → speeds up filtering by year/date ranges.
--Index on employee_id → improves grouping/aggregations per employee.
--Best option: Composite index (employee_id, pay_date) to cover both filtering and grouping efficiently.

--6. Inefficient query

SELECT * FROM Payroll WHERE YEAR(pay_date) = 2024;
--Problem: YEAR(pay_date) applies a function to the column, preventing index usage → full table scan.

Optimized:
SELECT * 
FROM Payroll
WHERE pay_date >= '2024-01-01' AND pay_date < '2025-01-01';
--Uses range filter, allowing index seek on pay_date.

--Section 4: Bonus
--7. Rank employees by total net salary within department

sql
WITH SalaryTotals AS (
    SELECT 
        e.employee_id,
        e.name,
        e.department,
        SUM(p.net_salary) AS total_net_salary
    FROM Employees e
    JOIN Payroll p ON e.employee_id = p.employee_id
    WHERE YEAR(p.pay_date) = 2024
    GROUP BY e.employee_id, e.name, e.department
)
SELECT 
    department,
    name,
    total_net_salary,
    RANK() OVER (PARTITION BY department ORDER BY total_net_salary DESC) AS dept_rank
FROM SalaryTotals
ORDER BY department, dept_rank;
