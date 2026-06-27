# 🎯 BackendM1GL - Système de Gestion d'Utilisateurs

Projet M1 Génie Logiciel - API REST avec authentification JWT

## 🛠️ Technologies

- ![.NET]
- ![C#]
- ![PostgreSQL]
- ![JWT]
- ![Swagger]

## ✨ Fonctionnalités

- 🔐 Authentification JWT avec Refresh Tokens
- 👤 Inscription / Connexion / Déconnexion
- 👥 Gestion des utilisateurs (CRUD)
-    Gestion des produits (CRUD)
- 🎭 Système de rôles (User / Admin)
- 🔒 Hashage des mots de passe avec BCrypt
- 📊 Pagination et recherche
- 📋 Logs avec Serilog
- 📖 Documentation Swagger

## 🚀 Installation

### Prérequis

- .NET 10 SDK
- PostgreSQL 
- Visual Studio 2025
-
### Étapes

1. Cloner le repository
git clone https://github.com/[ton-username]/BackendM1GL.git
cd BackendM1GL
2. Restaurer les packages
dotnet restore

3. Configurer la base de données
   - Créer une base PostgreSQL nommée \`usermgmt\`
   - Modifier la connection string dans \`appsettings.json\`

4. Appliquer les migrations

dotnet ef database update

5. Lancer l'application

dotnet run

6. Accéder à Swagger

https://localhost:7xxx/swagger

## 📚 Endpoints principaux

### 🔐 Authentification
- \`POST /api/auth/register\` - Inscription
- \`POST /api/auth/login\` - Connexion
- \`POST /api/auth/refresh\` - Renouveler token
- \`POST /api/auth/logout\` - Déconnexion

### 👥 Utilisateurs
- \`GET /api/users/me\` - Mon profil
- \`PUT /api/users/me\` - Modifier mon profil
- \`GET /api/users\` - Liste (Admin)
- \`POST /api/users\` - Créer (Admin)
- \`DELETE /api/users/{id}\` - Supprimer (Admin)
-
### 📦 Produits

- GET | \`/api/products\` | Liste des produits 
- GET | \`/api/products/{id}\` | Détail produit 
- POST | \`/api/products\` | Créer produit 
- PUT | \`/api/products/{id}\` | Modifier produit 
- DELETE | \`/api/products/{id}\` | Supprimer produit 

## 👥 Équipe

- Aboulma-ali Abdoulaye -- Abdoukarim Thiam -- Marieme Diene - Développeur
