# Návrh architektúry mikroservisov pre systém TransitPay

## Prehľad

TransitPay bude navrhnutý ako **cloud-native mikroservisná architektúra** s dôrazom na princíp **single-responsibility** (každý komponent má jasne definovanú jednu zodpovednosť).

- **Komunikácia medzi službami** bude **event-driven** cez **Azure Service Bus**, čo zabezpečí **loose coupling** a **asynchrónnu výmenu správ**.
- **Frontend aplikácie** bude implementovaný v **Blazor WebAssembly** (SPA v prehliadači).
- **Backend mikroservisy** budú postavené na **.NET**, s využitím **.NET Aspire** na orchestráciu a lokálny beh distribuovaných komponentov počas vývoja.
- **Nasadenie prebehne v kontajneroch** orchestrovaných cez **Kubernetes (napr. Azure Kubernetes Service - AKS)** pre zabezpečenie **škálovateľnosti** a **vysokej dostupnosti**.

---

## Mikroservisy a ich zodpovednosti

Navrhované mikroservisy v súlade s doménou **TransitPay** (platby za verejnú dopravu) sú definované tak, aby každý plnil **jednu hlavnú funkciu**. Názvy služieb odrážajú ich **biznis účel** podľa osvedčených postupov.

### **1. Identity (Autentizácia a autorizácia)**
- Zabezpečuje správu identity používateľov – prihlasovanie, registráciu a vydávanie tokenov.
- Integruje sa s **Azure AD B2C** alebo inou identitnou platformou pre jednotné prihlásenie.

### **2. User Profile (Správa používateľov)**
- Udržuje profilové údaje cestujúcich (**meno, kontakty, preferencie**).
- Plne zodpovedá za **dáta používateľov** a neprekrýva sa s inými službami.

### **3. Trip Management (Správa jázd)**
- Spracováva udalosti súvisiace s jazdou cestujúceho.
- Pri **“check-in”/“check-out”** vytvorí záznam o jazde a sleduje trasu alebo zóny.
- Na konci publikuje udalosť **TripEnded**.

### **4. Fare Calculation (Výpočet cestovného)**
- Po prijatí udalosti **TripEnded** vypočíta tarifnú cenu podľa obchodných pravidiel.
- Publikuje udalosť **FareCalculated** s požadovanou sumou platby.

### **5. Payment Processing (Spracovanie platieb)**
- Po prijatí udalosti **FareCalculated** vykoná **ACID transakciu** v databáze.
- Integruje sa s platobnými systémami (napr. **Stripe/PayPal API**).
- Publikuje udalosť **PaymentCompleted** (prípadne **PaymentFailed**).

### **6. Notification (Notifikačný servis)**
- Odosiela **notifikácie** a **potvrdenia**.
- Po úspešnej platbe odoberie **PaymentCompleted** a zašle elektronický lístok (email/SMS/push).

### **7. API Gateway (Brána rozhraní)**
- Poskytuje jednotný vstupný bod pre **front-end (Blazor WASM)** a externé klienty.
- Realizuje **autorizáciu, routing** a **agregáciu dát**.

> **Poznámka:** Každý mikroservis má **vlastnú databázu**, optimalizovanú pre jeho potreby.

---

## **Event-driven komunikácia s Azure Service Bus**

Mikroservisy **nekomunikujú priamo synchronnými REST volaniami**, namiesto toho využívajú **publikovanie a odoberanie udalostí cez Azure Service Bus** (**pub/sub model**).

- Pri **zmenách stavov** (napr. ukončenie jazdy, dokončenie výpočtu ceny, úspešná platba) mikroservisy **publikujú integračné eventy**, ktoré sú **asynchrónne spracované inými službami**.
- **Event Flow príklad:**
  1. **Trip Management** publikuje **TripEnded**.
  2. **Fare Calculation** ho odoberie a publikuje **FareCalculated**.
  3. **Payment Processing** odoberie **FareCalculated**, zrealizuje platbu a následne publikuje **PaymentCompleted** alebo **PaymentFailed**.
  4. **Notification** odoberá tieto udalosti a odosiela potvrdenia.

> **Azure Service Bus** slúži ako spoľahlivý **message broker**, podporujúci **fronty, topics a subscriptions**.

---

## **Transakčná konzistencia a ACID**

V distribuovanej architektúre je dôležité **rozlíšiť, kedy potrebujeme silnú konzistenciu (ACID) a kedy stačí eventuálna konzistencia**.

- Každý mikroservis vykonáva **lokálne ACID transakcie** v rámci **vlastnej databázy**.
- Naprieč viacerými mikroservismi sa **používajú vzory ako Saga** pre zaistenie konzistencie.

Príklad použitia **Saga Pattern**:
- Pri zlyhaní platby (**PaymentFailed**) môže mechanizmus **Saga** vyvolať **kompenzačné akcie**, ako napríklad označenie jazdy ako **nezaplatenej**.

---

## **Škálovateľnosť a vysoká dostupnosť**

- Každý mikroservis beží ako **nezávislý kontajner (Docker image)** v **Kubernetes klastroch (AKS)**.
- Podporuje **Horizontal Pod Autoscaler (HPA)** pre **automatické škálovanie podľa CPU, RAM, front v Service Bus a pod.**.
- **Bezstavové služby** uchovávajú stav v **databázach alebo cache**, nie v pamäti inštancií.

> **CI/CD nasadenie** využíva **Azure DevOps** alebo **GitHub Actions**.

---

## **Zhrnutie**

TransitPay architektúra:
- **Osvedčené princípy Microsoftu pre mikroservisy**.
- **Asynchrónna komunikácia** cez **Azure Service Bus**.
- **Škálovateľnosť (AKS) a spoľahlivosť (ACID + eventuálna konzistencia)**.

Architektúru treba **neustále vyhodnocovať a prispôsobiť reálnym potrebám** – ak by počet mikroservisov **zbytočne zvyšoval komplexnosť či latenciu**, je možné **niektoré funkcie zlučovať**.

> **Moderné architektúry fungujú najlepšie, keď nájdu balans medzi čistými princípmi a praktickými kompromismi.**