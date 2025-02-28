Systém na zúčtovanie cestovných lístkov vo verejnej doprave 
spracováva transakcie cestujúcich – či už ide o jednorazové lístky, 
predplatné alebo rôzne zľavy. 

Zúčtovanie prebieha raz mesačne, čo naznačuje, 
že sa robí mesačný settlement (vyrovnanie) medzi 
rôznymi poskytovateľmi (dopravcami, bankami, platobnými bránami).

🔹 Cestujúci platia rôznymi spôsobmi (karty, mobilné aplikácie, SMS, predplatné)
🔹 Dopravcovia potrebujú spravodlivé rozdelenie tržieb medzi rôznymi regiónmi a spoločnosťami
🔹 Niektoré transakcie sa spracovávajú oneskorene (napr. reklamácie, refundácie)

### Návrh Microservices Architektúry

🏗 1 TicketingMicroservice (Predaj lístkov)
Zodpovednosť: Spravuje vydané lístky, ich validitu a históriu.
Funkcionalita:
Predaj lístkov cez mobilnú appku, web a predajné automaty.
Generovanie a validácia QR kódov pre lístky.
API pre zariadenia vo vozidlách (kontrola lístkov).
Komunikácia: Prijíma požiadavky od Blazor frontendu, poskytuje dáta pre SettlementMicroservice.
Databáza: PostgreSQL / SQL Server.