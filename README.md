# 🌸 GlamStudio - System rezerwacji i zarządzania salonem kosmetycznym

![.NET](https://img.shields.io/badge/.NET-Core-purple?style=flat-square)
![Database](https://img.shields.io/badge/Database-SQL%20Server-lightgrey?style=flat-square)
![PWA](https://img.shields.io/badge/PWA-Ready-blue?style=flat-square)

> **Niniejsze repozytorium zawiera kod źródłowy aplikacji webowej, która wspiera zarządzanie salonem kosmetycznym. Aplikacja ta stanowi element pracy inżynierskiej.**

System umożliwia klientom rezerwację wizyt online 24/7, a pracownikom zapewnia narzędzia do zarządzania grafikiem, bazą klientów oraz analizą finansową.

Aplikacja została zaimplementowana w architekturze **MVC** i spełnia standardy **PWA (Progressive Web App)**, dzięki czemu można ją zainstalować na urządzeniach mobilnych.

---

## 🚀 Kluczowe Funkcjonalności

### 👤 Strefa Klienta
* **Rezerwacja Online:** Intuicyjny kreator wizyt z wyborem usługi, pracownika i terminu.
* **Profil Użytkownika:** Historia wizyt, możliwość samodzielnego anulowania rezerwacji, edycja danych osobowych.
* **Responsywność:** Pełne dostosowanie do smartfonów i tabletów.
* **Instalacja PWA:** Możliwość dodania aplikacji do ekranu głównego telefonu (bez AppStore/Google Play).

### 🏢 Strefa Salonu
* **Inteligentny Grafik:** Interaktywny kalendarz pracy personelu (zintegrowany z `FullCalendar.js`).
* **Zarządzanie Wizytami:** Weryfikacja dostępności terminów w czasie rzeczywistym (zapobieganie *double-booking*), zmiana statusów wizyt.
* **Baza CRM:** Kartoteki klientów z historią zabiegów i prywatnymi notatkami dla personelu (np. o alergiach).
* **Raporty i Analizy:** Wizualizacja przychodów i popularności usług na wykresach (`Chart.js`).
* **Administracja:** Zarządzanie pracownikami, usługami, kategoriami i uprawnieniami.

---

## 🛠️ Wykorzystane Technologie

Projekt zrealizowano przy użyciu nowoczesnego stosu technologicznego Microsoft:

| Obszar | Technologie |
| :--- | :--- |
| **Backend** | C#, ASP.NET Core MVC, Entity Framework Core (Code-First) |
| **Baza Danych** | Microsoft SQL Server |
| **Frontend** | Razor Views, HTML5, CSS3, JavaScript |
| **Framework UI** | Bootstrap 5 (RWD - Mobile First) |
| **Biblioteki JS** | FullCalendar (grafik), Chart.js (statystyki) |
| **Bezpieczeństwo** | ASP.NET Core Identity (Role: Admin, Employee, Client) |

---

## 📄 Autor

**Emilia Sordyl** Studentka kierunku Informatyka  

## Promotor

**dr inż. Paweł Fałat** 

