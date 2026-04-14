Payment Reporting System – Design & Implementation Specification
Objective

Analyze the existing payment system flow and design a robust, flexible reporting module that provides actionable financial insights across different dimensions.

Core Reporting Requirements
1. Class-Level Reports
Generate reports per class
Apply filters for:
Session
Term
Payment Category (single or all categories)
Output should clearly show:
Expected vs Actual payments
Number of students who have paid
Outstanding payments
2. School-Wide Reports
Generate reports for the entire school (all classes)
Data should be:
Aggregated across all classes
Clearly grouped/separated by payment category
Should provide:
Total revenue per category
Overall payment performance
3. Category Item-Level Reports
Generate reports for individual payment items (e.g., Tuition, PTA Levy, Exam Fee)
Each report must include:
Total number of students who paid for that item
Total amount collected
Payment distribution (optional but recommended)
Report Structure & UI Design
Reports may be implemented across multiple dedicated pages for better usability and filter clarity:
Class Report Page
School Summary Report Page
Category/Item Report Page
Each page should include:
Advanced filtering (Session, Term, Class, Category)
Summary cards (totals, counts, etc.)
Tabular data presentation
Data Export Requirements

All reports must support export functionality using DataTables integration, including:

Excel (.xlsx)
CSV (.csv)
PDF (.pdf)
Print View

Ensure:

Proper formatting in exports
Headers and summaries are included
Large datasets are handled efficiently
Implementation Notes
Reuse existing payment logic to ensure consistency
Optimize queries for performance (especially for aggregated reports)
Ensure reports are accurate and reflect real-time data
Maintain clean separation between:
Data retrieval (services/repositories)
Presentation (views/UI)