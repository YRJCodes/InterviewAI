# InterviewPrep AI - Migration & Setup Guide

This project has been migrated from a Node.js/Express/MongoDB stack to an **ASP.NET Core** and **MySQL** stack. Follow the instructions below to set up and run the application.

## Prerequisites

- **.NET SDK**: .NET 8.0 or 10.0
- **MySQL Server**: Version 8.0 or higher
- **Node.js**: Version 18.x or higher
- **NPM** or **Bun**

---

## 1. Database Setup

The backend uses Entity Framework Core with a "Code First" approach. The database and tables will be created automatically on the first run.

1. **Start MySQL Server**: Ensure your MySQL service is running.
2. **Create Database**: (Optional) The application is configured to use a database named `interviewzwt`. You can create it manually if desired:
   ```sql
   CREATE DATABASE interviewzwt;
   ```
3. **Verify Credentials**: Open `Interviewzwt.Backend/appsettings.json` and ensure the connection string matches your local MySQL setup:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost;port=3306;database=interviewzwt;user=root;password=YOUR_PASSWORD"
   }
   ```

---

## 2. Backend Configuration

1. **Navigate to the Backend Directory**:
   ```bash
   cd Interviewzwt.Backend
   ```
2. **Configure API Keys**: Open `appsettings.json` and provide your external service keys:
   - `Groq:ApiKey`: For AI analysis and scoring.
   - `AssemblyAI:ApiKey`: For voice-to-text transcription.
   - `PayPal:ClientId` & `ClientSecret`: For credit purchases.
3. **Run the Backend**:
   ```bash
   dotnet run --urls=http://localhost:5000
   ```
   *The first run will take a moment as it initializes the database schema and seeds default job roles.*

---

## 3. Frontend Configuration

1. **Navigate to the Frontend Directory**:
   ```bash
   cd Interviewzwt
   ```
2. **Install Dependencies**:
   ```bash
   npm install
   ```
3. **Verify API Endpoint**: The frontend is configured to call the backend at `http://localhost:5000/api`. This is managed in `src/integrations/api/client.ts`.
4. **Run the Frontend**:
   ```bash
   npm run dev
   ```

---

## 4. Key Features & Endpoints

- **Auth**: `POST /api/auth/register`, `POST /api/auth/login`
- **Job Roles**: `GET /api/job-roles` (Seeded automatically)
- **Resume Analysis**: `POST /api/functions/analyze-resume`
- **Interview Scoring**: `POST /api/functions/score-interview`
- **Voice Interview**: `POST /api/functions/voice-interview`
- **Swagger Documentation**: Access `http://localhost:5000/swagger` to view and test all API endpoints in your browser.

---

## Troubleshooting

- **Database Connection**: If you get a connection error, verify that MySQL is allowed to accept connections on port 3306 and that the user/password in `appsettings.json` are correct.
- **Port Conflict**: If port 5000 is occupied, you can change the URL in the `dotnet run` command or update `Properties/launchSettings.json`.
- **Missing DLLs**: If you see "File Not Found" errors during `dotnet run`, ensure you have the matching .NET Runtime installed (`dotnet --list-runtimes`).
