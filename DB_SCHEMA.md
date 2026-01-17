# DB Schema Kurulumu

Bu dokuman, veritabani semasinin nasil kuruldugunu ve projede nasil modellendigini aciklar.

## 1) Varliklar ve iliskiler

Hiyerarsi:
- Board
  - Columns[]
  - Cards[]
  - Comments[]
  - Participants[]

Iliskiler:
- Board 1 - N Participant
- Board 1 - N Column
- Column 1 - N Card
- Card 1 - N Comment

## 2) Entity siniflari

Entity'ler su klasorde:
- `Domain/Entities/Board.cs`
- `Domain/Entities/Participant.cs`
- `Domain/Entities/Column.cs`
- `Domain/Entities/Card.cs`
- `Domain/Entities/Comment.cs`

Her entity mockBoards.js ile birebir alanlari tasir (coklu board icin composite key destekli):
- Board: Id (string), Name, Date, InviteRequired
- Participant: Id (int), Name, BoardId (string)
- Column: Id (string), Title, BoardId (string)
- Card: Id (string), Text, Votes, BoardId (string), ColumnId (string)
- Comment: Id (string), Author, Text, CreatedAt, BoardId (string), ColumnId (string), CardId (string)

## 3) DbContext ve Fluent API

DbContext:
- `Infrastructure/Data/RetroboardDbContext.cs`

OnModelCreating icinde iliskiler ve anahtarlar tanimlandi:
- Board, Column, Card, Comment icin string Id kullanildigi icin `ValueGeneratedNever()`.
- Column icin composite primary key: `{BoardId, Id}`.
- Card icin composite primary key: `{BoardId, ColumnId, Id}`.
- Comment icin composite primary key: `{BoardId, ColumnId, CardId, Id}`.
- Board -> Participants ve Board -> Columns icin `HasMany / WithOne` + `OnDelete(DeleteBehavior.Cascade)`.
- Column -> Cards ve Card -> Comments icin `HasMany / WithOne` + `OnDelete(DeleteBehavior.Cascade)`.
- Zorunlu alanlar `IsRequired()` ile isaretlendi.

Bu kurulum sayesinde EF Core dogru tablolar ve foreign key'ler uretir.

## 4) Connection String

`appsettings.json` icinde DefaultConnection vardir:
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NewNA;User Id=;Password=;TrustServerCertificate=True"
  },
  "Urls": "http://*:1330"
}
```

Not: Gercek sifreler repoya konmamalidir.

## 5) Migration olusturma

Kurallar (AGENTS.md ile uyumlu):
- Migration olustur:
  - `dotnet ef migrations add InitSchema -s Api -p Infrastructure`
- Veritabanina uygula:
  - `dotnet ef database update -s Api -p Infrastructure`

Bu komutlar, DbContext'teki modeli okuyup migrations klasorune dosya uretir ve veritabaninda tablo, FK, index gibi objeleri olusturur.
Composite key degisikligi yaptigin icin yeni migration olusturman gerekir.

## 6) Migration dosyalari nereye yazilir?

Varsayilan olarak EF Core migrations dosyalari API projesinin altina olusur:
- `Migrations/`

Eger farkli bir konum istiyorsan, komuta `-o` parametresi ekleyebilirsin:
- `dotnet ef migrations add InitSchema -s Api -p Infrastructure -o Infrastructure/Migrations`

## 7) Kontrol listesi

- `Infrastructure/Data/RetroboardDbContext.cs` icinde DbSet'ler var mi?
- Connection string dogru mu?
- `dotnet ef migrations add InitSchema -s Api -p Infrastructure` basariyla calisti mi?
- `dotnet ef database update -s Api -p Infrastructure` DB'ye uygulandi mi?
