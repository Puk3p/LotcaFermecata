# Fit-Pass

## Project Description

This project includes:
- A **.NET 8 Web API** as the backend.
- An **Angular v19** application as the frontend.

The backend uses a **layered architecture**, which means the code is split into separate parts (layers), each with its own responsibility:

- `SP25.WebApi` – This is the **API layer** and serves as the **entry point** of the application for the client (frontend). It handles HTTP requests and sends responses.
- `SP25.Business` – This is the **business logic layer**, where the main logic of the application is written.
- `SP25.Domain` – This layer contains the **domain models** and is responsible for **communicating with the database**.

This architecture helps keep the code clean, organized, and easy to understand and maintain.

---

## How to Run the API (.NET 8)

1. Open the solution in **Visual Studio**.
2. In the **Solution Explorer**, right-click on `SP25.WebApi` and choose **"Set as Startup Project"**.
3. Build the solution with **Ctrl+Shift+B**.
4. Run the project using **F5** (with debugger) or **Ctrl+F5** (without debugger).

---

## How to Run the Angular Project

1. Open the Angular project folder in **Visual Studio Code**.
2. Open a terminal in VS Code (**Ctrl+`** or from the menu: **Terminal > New Terminal**).
3. Run the following commands:

   ```bash
   npm install
   ng serve
