# Enterprise Reporting Platform

A production-style enterprise reporting, analytics, and data integration platform built with .NET 10, ASP.NET Core, Blazor, Entity Framework Core, REST APIs, and Docker.

## Overview

The Enterprise Reporting Platform demonstrates a full-stack enterprise application for managing customers, products, orders, reporting dashboards, and external sales-data integration.

The system provides REST APIs, a Blazor web dashboard, database persistence, CSV ingestion and validation, integration job tracking, automated tests, Docker support, and CI/CD.

## Key Features

- Enterprise reporting dashboard
- Customer management
- Product management
- Order management
- Revenue and sales analytics
- Regional sales reporting
- CSV sales-data ingestion
- Data validation and error tracking
- Duplicate transaction detection
- Integration job monitoring
- REST API architecture
- Entity Framework Core persistence
- Blazor web interface
- Docker containerization
- Automated unit and integration tests
- GitHub Actions CI pipeline

## Architecture

```text
                    Blazor Web Application
                             |
                             v
                    ASP.NET Core REST API
                             |
          +------------------+------------------+
          |                  |                  |
          v                  v                  v
     Customer APIs       Order APIs        Product APIs
          |                  |                  |
          +------------------+------------------+
                             |
                             v
                    Entity Framework Core
                    ReportingDbContext
                             |
                             v
                    Reporting Database
                 SQLite Local Development
