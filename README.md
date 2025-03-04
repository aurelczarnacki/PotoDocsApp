

# 📦 PotoDocs  

<img src="Screenshots/Screeenshot_login.png" height=385 width=211 align=right>

🚛 **PotoDocs** is a transport order management system that allows users to store, manage, and process transport orders, generate invoices using an AI-based API, and handle document storage and user roles.  

💡 The application is **fully responsive**, meaning it works seamlessly on both desktop and mobile devices.  

⚪🔴 This application was developed only in the Polish language version.

### 📂 Project Structure  
PotoDocs 

  │── 📂 PotoDocs.API # Backend API (database connection, business logic) 
  
  │── 📂 PotoDocs.Shared # Shared DTOs for API and frontend 
  
  │── 📂 PotoDocs.Blazor # Frontend (Blazor)  
  
  
  



## 🔥 Key Features  

<img src="Screenshots/Screeenshot_allOrders.png" height=385 width=211 align=left>

✅ Store and manage transport orders.

✅ Calculation and display of order status

✅ Generate invoices based on orders (via GPT-powered API). 

✅ Store and manage transport-related documents.

✅ User role management with different access levels.

✅ Auto-generated passwords sent via email.

✅ Download invoices for a specific time period.

✅ **Fully responsive UI** – works on both **desktop & mobile**.

✅ Frontend designed based on a **Figma prototype**.  

---

## 🛠 Tech Stack  
<p align=center> 
  <img src="Screenshots/Icons/csharp.svg.png" width=90 height=100>
&nbsp;
  &nbsp;
<img src="Screenshots/Icons/Net.svg.png" width=100 height=100>
&nbsp;&nbsp;
<img src="Screenshots/Icons/Blazor.png" width=100 height=100 style="margin-right: 20">
  &nbsp;&nbsp;
<img src="Screenshots/Icons/html.svg.png" width=100 height=100>
&nbsp;&nbsp;
<img src="Screenshots/Icons/css.svg.png" width=70 height=100>
&nbsp;&nbsp;
<img src="Screenshots/Icons/gpt.png" width=100 height=100>
&nbsp;&nbsp;
<img src="Screenshots/Icons/mariadb.png" width=100 height=100>

</p>
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
