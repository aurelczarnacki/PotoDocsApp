# 📦 PotoDocs  

🚛 **PotoDocs** is a transport order management system that allows users to store, manage, and process transport orders, generate invoices using an AI-based API, and handle document storage and user roles.  

💡 The application is **fully responsive**, meaning it works seamlessly on both desktop and mobile devices.  

![Login page](Screenshots/Screenshot_login.png)

## 🔥 Key Features  
✅ Store and manage transport orders.  
✅ Generate invoices based on orders (via GPT-powered API).  
✅ Store and manage transport-related documents.  
✅ User role management with different access levels.  
✅ Auto-generated passwords sent via email.  
✅ Download invoices for a specific time period.  
✅ **Fully responsive UI** – works on both **desktop & mobile**.  
✅ Frontend designed based on a **Figma prototype**.  

---

## 🛠 Tech Stack  
- **Backend**: .NET, MariaDB (MySQL)  
- **Frontend**: Blazor  
- **Shared Components**: DTOs shared between frontend and backend  

### 📂 Project Structure  
PotoDocs 
  │── PotoDocs.API # Backend API (database connection, business logic) 
  │── PotoDocs.Shared # Shared DTOs for API and frontend 
  │── PotoDocs.Blazor # Frontend (Blazor)

---

## 🚀 Installation & Setup  

### 1️⃣ Clone the Repository  
```bash
git clone https://github.com/your-username/PotoDocs.git
cd PotoDocs
2️⃣ Backend Setup
Install .NET SDK
Configure MariaDB/MySQL and update connection settings in appsettings.json
Run migrations (if applicable):
bash
Kopiuj
Edytuj
dotnet ef database update
Start the API:
bash
Kopiuj
Edytuj
cd PotoDocs.API
dotnet run
3️⃣ Frontend Setup
Install dependencies (if needed)
Run the Blazor app
bash
Kopiuj
Edytuj
cd PotoDocs.Blazor
dotnet run
