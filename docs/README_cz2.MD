# **TransitPay – Ticketing & Payment Settlement System**
🚆 **Enterprise-grade systém pre predaj cestovných lístkov, spracovanie transakcií a zúčtovanie platieb vo verejnej doprave.**  

## 📌 **Prehľad projektu**
**TransitPay** je distribuovaný systém určený na **spracovanie transakcií a zúčtovanie cestovných lístkov** vo verejnej doprave.  
Systém integruje **platobné brány, predajné zariadenia, bankové systémy a účtovné platformy**, pričom poskytuje **spoľahlivé a bezpečné transakčné spracovanie**.

- 🏗 **Architektúra:** Clean Architecture + Microservices  
- 🌍 **Frontend:** Blazor WebAssembly  
- 🔗 **Backend:** .NET 8, .NET Aspire  
- ☁️ **Cloudová infraštruktúra:** Azure, Kubernetes, Docker  
- 🔄 **Komunikácia mikroservisov:** Azure Service Bus (event-driven architecture)  
- 🛠 **Dátové úložiská:** SQL Server, PostgreSQL, Redis, Blob Storage  

---

## 🚀 **Architektúra systému**
TransitPay je postavený na **moderne škálovateľnej mikroservisnej architektúre** s oddelenými doménami.  
Každý mikroservis je nezávislý a komunikuje cez **REST API, gRPC a event-driven messaging**.

### 🏗 **Mikroservisy**
| Mikroservis                    | Zodpovednosť                                    | Technológie               |
|--------------------------------|-------------------------------------------------|---------------------------|
| **TicketingMicroservice**      | Správa cestovných lístkov a ich validácia       | .NET 8, PostgreSQL        |
| **TransactionMicroservice**    | Evidencia platieb a transakcií                  | .NET 8, SQL Server        |
| **PaymentGatewayMicroservice** | Integrácia s bankovými systémami                | .NET 8, Stripe API, GoPay |
| **SettlementMicroservice**     | Mesačné vyrovnanie tržieb medzi dopravcami      | .NET 8, SQL Server        |
| **POSMicroservice**            | API pre predajné zariadenia (terminály, kiosky) | .NET 8, MongoDB           |
| **NotificationMicroservice**   | E-maily, SMS, push notifikácie                  | .NET 8, Redis             |

### 📡 **Komunikácia medzi mikroservismi**
✔ **REST API** – pre synchronné požiadavky (Ticketing, Transaction, POS)  
✔ **gRPC** – pre rýchlu medziservisnú komunikáciu (Settlement, PaymentGateway)  
✔ **Azure Service Bus** – event-driven messaging pre škálovateľnosť  

---

## 🏗 **Technológie a stack**
TransitPay využíva **enterprise-grade technológie**, ktoré umožňujú škálovateľnosť a vysokú dostupnosť.

### **🖥 Frontend**
- **Blazor WebAssembly** – Rýchly klientský frontend  
- **.NET 8 + C#** – Single Page Application (SPA)  
- **TailwindCSS / MudBlazor** – UI komponenty  

### **🔧 Backend**
- **.NET 8** – Microservices API  
- **.NET Aspire** – Orchestrácia mikroservisov  
- **MediatR** – CQRS pre request handling  
- **FluentValidation** – Validácia requestov  
- **Serilog** – Logging a monitoring  

### **💾 Dátové úložiská**
- **SQL Server / PostgreSQL** – ACID transakcie  
- **MongoDB** – NoSQL ukladanie pre POS dáta  
- **Redis** – Cache + event storage pre notifikácie  
- **Azure Blob Storage** – Ukladanie reportov a faktúr  

### **☁️ Cloud & Deployment**
- **Docker + Kubernetes** – Kontajnerizácia a orchestrácia  
- **Azure Service Bus** – Messaging medzi mikroservismi  
- **Azure API Management** – Centrálna správa API endpointov  
- **CI/CD (GitHub Actions, Azure Pipelines)** – Automatizované buildy a nasadzovanie  

---

## 📜 **Hlavné Use Cases**
1️⃣ **Cestujúci si kúpi lístok cez mobilnú appku (Blazor WebAssembly)**  
2️⃣ **Platba prejde cez TransactionMicroservice a PaymentGatewayMicroservice**  
3️⃣ **Notifikačný systém odošle potvrdenie (SMS, e-mail, push notifikácia)**  
4️⃣ **Mesačné zúčtovanie v SettlementMicroservice rozdelí tržby medzi dopravcov**  
5️⃣ **Predajné terminály (POS) synchronizujú dáta cez POSMicroservice**  

---

## 🛠 **Ako spustiť projekt lokálne**
1️⃣ **Naklonuj si repozitár**  
```sh
git clone https://github.com/your-repo/TransitPay.git
cd TransitPay
```
2️⃣ **Spustenie kontajnerov s databázami**  
```sh
docker-compose up -d
```
3️⃣ **Spustenie mikroservisov cez .NET Aspire**
```sh
dotnet run --project src/AppHost
```
4️⃣ **Otvor frontend (Blazor WebAssembly)**
```sh
cd src/Presentation/BlazorApp
dotnet run
```
5️⃣ **Prístup k API dokumentácii (Swagger)**
```sh
http://localhost:5000/swagger
```

---

## 🎯 **Roadmap & Budúci vývoj**
- [ ] **Podpora pre OpenID Connect (OAuth 2.0, Azure AD)**  
- [ ] **Grafy a reporting v Blazor WebAssembly**  
- [ ] **Podpora pre NFC a QR skenovanie v POS termináloch**  
- [ ] **Machine Learning na detekciu podvodných transakcií**  

---

## 🤝 **Prispievanie do projektu**
💡 **Chceš sa zapojiť?** Vítame pull requesty!  
✅ **Štandardy kódu:** Dodržuj Clean Architecture, SOLID princípy a CQRS pattern.  
✅ **Testovanie:** Pokrytie minimálne **80% unit testami (xUnit, Moq, FluentAssertions)**.  
✅ **Code review:** Každý PR prechádza **peer review procesom pred merge**.  

---

## 📧 **Kontakt a podpora**
🔹 **Lead Software Engineer / Architect:** [Tvoje meno]  
🔹 **E-mail:** tvojemeno@transitpay.com  
🔹 **GitHub Issues:** [Otvoriť Issue](https://github.com/your-repo/TransitPay/issues)  

🚀 **Ak máš otázky alebo návrhy na zlepšenie, pridaj issue alebo pull request!** 🚀