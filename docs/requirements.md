# Requirements

Develop a RESTful web service in C# that securely processes uploaded files and returns the results. The
service should support either CSV or JSON (your choice) file formats and include an API Key-based
security mechanism. Additionally, it should track the files processed and provide basic reporting of these
files upon request. The goal is not necessarily to fully complete the work, but to give a solid
representation of how you accomplish the work.

## 1. Secure Web Service

- Implement the service using ASP.NET Core.
- Secure the file upload endpoint with an API key authentication mechanism.

## 2. File Processing

- Support either CSV or JSON file formats.
- For CSV, calculate a simple aggregate, such as the average of a specific column.
- For JSON, perform a simple data transformation, such as filtering based on a condition.

## 3. API Key Authentication

- Implement a basic API key check using middleware.
- Validate the API key sent in the request header.

## 4. File Tracking and Reporting

- Implement a simple counter to track the number of files processed.
- Alternatively, log basic file processing details, such as the filename and processing time.

## 5. Containerization

- Containerize the service using Docker.
- Provide a Dockerfile for building and running the container.

## 6. Documentation

- Document API endpoints and usage in a `README.md` file.
- Include instructions for building, running, and using the service.

## 7. Code Quality

- Write clean, readable, and maintainable code.
- Include error handling and logging.
