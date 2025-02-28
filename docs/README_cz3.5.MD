## 📌 Návrh architektúry mikroservisov pre systém TransitPay

### **Prehľad:**  
TransitPay bude navrhnutý ako cloud-native **mikroservisná architektúra** s dôrazom na princíp **single-responsibility** (každý komponent má jasne definovanú jednu zodpovednosť).
Komunikácia medzi službami bude **udalostne riadená (event-driven)** cez **Azure Service Bus**, čo zabezpečí voľné prepojenie (loose coupling) a asynchrónnu výmenu správ.
Frontend aplikácie bude implementovaný v **Blazor WebAssembly** (SPA v prehliadači)/**Blazor Web** a backend mikroservisy v .NET, s využitím **.NET Aspire** na orchestráciu a lokálny beh distribuovaných komponentov počas vývoja.
Nasadenie prebehne v kontajneroch orchestrovaných cez **Kubernetes** (napr. Azure Kubernetes Service) pre zabezpečenie škálovateľnosti a vysokej dostupnosti.

## 📡 Mikroservisy a ich zodpovednosti  
Navrhované mikroservisy v súlade s doménou TransitPay (platby za verejnú dopravu) sú definované tak, aby každý plnil jednu hlavnú funkciu.
Názvy služieb odrážajú ich biznis účel podľa osvedčených postupov Microsoftu.

### 🏗 **Navrhnuté mikroservisy**  
| Mikroservis                    | Zodpovednosť                                                            |
|--------------------------------|-------------------------------------------------------------------------|
| **Identity Service**           | Autentifikácia a autorizácia (OAuth 2.0, OpenID Connect, Azure AD B2C)  |
| **User Profile Service**       | Správa profilov používateľov                                            |
| **Trip Management Service**    | Evidencia ciest, správa „check-in“/„check-out“ transakcií               |
| **Fare Calculation Service**   | Výpočet cestovného na základe trasy, vzdialenosti a tarifných pravidiel |
| **Payment Processing Service** | Integrácia s platobnými bránami, správa transakcií                      |
| **Notification Service**       | Odosielanie e-mailov, SMS a push notifikácií                            |
| **API Gateway**                | Zjednodušený prístup cez jednotný endpoint, routing requestov           |

---

## 📜 **Event-Driven Komunikácia s Azure Service Bus**  
TransitPay využíva **Azure Service Bus** na **asynchrónnu komunikáciu** medzi službami.
To zabezpečí **decoupling** a vyššiu škálovateľnosť.

🔹 **Príklad udalostného toku:**  
1️⃣ **Trip Management Service** publikuje `TripEnded` event.  
2️⃣ **Fare Calculation Service** spracuje túto udalosť a vypočíta tarifnú cenu.  
3️⃣ **Payment Processing Service** dostane `FareCalculated` event a spracuje platbu.  
4️⃣ **Notification Service** po spracovaní platby odosiela e-mail/SMS potvrdenie.  

---

## 🚀 **Škálovateľnosť a vysoká dostupnosť**  
- **Kontajnerizácia s Dockerom**: Každý mikroservis je zabalený do kontajnera.  
- **Orchestrácia s Kubernetes (AKS)**: Dynamické škálovanie pomocou **Horizontal Pod Autoscaler (HPA)**.  
- **CI/CD pipelines**: GitHub Actions pre automatizované buildy a deploymenty.  
- **API Gateway**: Centralizovaný vstupný bod pre externých klientov (Azure API Management alebo YARP).  

---

## 🎯 **Alternatívne prístupy – Agregácia mikroservisov**  
Hoci čistá mikroservisná architektúra je odporúčaná, niektoré organizácie používajú **agregované služby** na zníženie latencie:  

✅ **Kedy agregovať mikroservisy?**  
- Ak dve služby **vždy spolu interagujú**, je možné ich spojiť a eliminovať sieťovú latenciu.  
- **Netflix API Gateway** agreguje odpovede z viacerých backend služieb do jedného requestu.  
- **Amazon využíva kombináciu mikroservisov a monolitov**, aby minimalizoval latenciu v kritických častiach systému.  

---

## 📢 **Zhrnutie**  
✔ TransitPay využíva **Microsoft best practices** pre mikroservisy.  
✔ **Azure Service Bus** zabezpečuje **event-driven messaging**.  
✔ **.NET Aspire** umožňuje jednoduchšiu orchestráciu mikroservisov počas vývoja.  
✔ **Kubernetes a AKS** zabezpečujú vysokú dostupnosť a škálovanie.  
✔ **Možnosť miernej agregácie služieb** na optimalizáciu výkonu.  

---
