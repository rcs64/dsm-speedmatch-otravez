# 🚀 Quick Start: DSM-SpeedMatch

> Guía rápida para empezar a usar la aplicación

## 5 Minutos para Entender Todo

### 1. Qué es DSM-SpeedMatch

```
Dating app inteligente con:
✅ Matching system (like + superlikes)
✅ Notificaciones en tiempo real
✅ Superlikes Premium (monetización)
✅ .NET 8 + NHibernate + Clean Architecture
```

### 2. 3 Conceptos Clave

```
📌 LIKE NORMAL
Usuario A → Like → Usuario B
           └─ B.LikesRecibidos += 1

📌 SUPERLIKE PREMIUM ⭐
Usuario Premium A → Superlike → Usuario B
                 └─ A.SuperlikesDisponibles -= 1
                 └─ B.LikesRecibidos += 2

📌 MATCH MUTUO 💑
Usuario A da Like → Usuario B acepta → MATCH MUTUO
                                    └─ Pueden chatear
```

---

## ⚙️ Instalación (2 minutos)

### Requisitos
```
✅ .NET SDK 8.0+
✅ SQL Server Express 2019+
✅ Visual Studio 2022 o VS Code
```

### Pasos

```bash
# 1. Clonar
git clone https://github.com/TaponDeBotella/DSM-SpeedMatch.git
cd DSM-SpeedMatch

# 2. Restaurar
dotnet restore

# 3. Compilar
dotnet build

# 4. Verificar
dotnet run --project InitializeDb
```

**Resultado esperado:**
```
✅ Build exitoso: 0 errores
✅ Tests: 60/60 exitosos
✅ JSON generado: CRUD_TEST_RESULTS.json
```

---

## 🏗️ Arquitectura en 1 minuto

```
Clean Architecture
├─ Presentación (Controller) → API
├─ Casos de Uso (CP) → SuperlikeCP, MatchCP
├─ Negocio (CEN) → SuperlikeCEN, MatchCEN
├─ Dominio (EN) → Usuario, Match, Notificacion
└─ Datos (Repos) → IUsuarioRepository, IMatchRepository

Regla de Oro:
Controller → CP (complejo) → CEN (simple) → Repos (BD)
```

---

## 🧠 CP vs CEN (lo más importante)

### SuperlikeCEN = Lógica simple

```csharp
// Operaciones independientes
bool PuedeHacerSuperlike(usuarioId)
int ObtenerSuperlikes(usuarioId)
void RestarSuperlike(usuarioId)
```

**Cuándo usas:** Desde CP, Tests, Services

### SuperlikeCP = Orquestación compleja

```csharp
// Caso de uso completo
Match Superlike(emisorId, receptorId)
    ├─ Validar Premium
    ├─ Validar tiene superlikes
    ├─ Crear Match
    ├─ Restar superlike
    ├─ Sumar 2 puntos
    ├─ Notificar
    └─ SaveChanges() - TODO O NADA
```

**Cuándo usas:** Desde Controller

### Analogía

```
CEN = Canción (notas individuales perfectas)
CP = Concierto (orquesta la canción completa)
```

---

## 💻 Ejemplos Prácticos

### Ejemplo 1: Dar un Like Normal

```csharp
// Desde Controller
var cp = new IniciarMatchCP(_matchCEN, _usuarioCEN, ...);
var match = cp.Iniciar(usuarioId: 3, receptorId: 5);

// Qué pasa:
// ✓ Match creado
// ✓ Usuario 5.LikesRecibidos += 1
// ✓ Notificación: "¡Usuario 3 te dio like!"
```

### Ejemplo 2: Dar un Superlike

```csharp
// Desde Controller
var cp = new SuperlikeCP(_matchCEN, _usuarioCEN, ...);
var match = cp.Superlike(emisorId: 3, receptorId: 5);

// Qué pasa:
// ✓ Validar: Usuario 3 es Premium
// ✓ Validar: SuperlikesDisponibles > 0
// ✓ Match creado (EsSuperlike = true)
// ✓ Usuario 3.SuperlikesDisponibles: 10 → 9
// ✓ Usuario 5.LikesRecibidos: 50 → 52 (DOBLE)
// ✓ Notificación especial
```

### Ejemplo 3: Comprar Superlikes

```csharp
// Desde Controller
var cp = new SuperlikeCP(...);
cp.ComprarSuperlikes(usuarioId: 3, cantidad: 5);

// Qué pasa:
// ✓ Usuario 3.SuperlikesDisponibles += 5
// ✓ [Aquí va integración Stripe]
```

---

## 🧪 Ejecutar Tests

```bash
cd InitializeDb
dotnet run
```

**Qué prueba:**

```
✅ 9 entidades (CRUD completo)
✅ 3 casos de uso (CP)
✅ 10 tests de superlikes (NUEVO)
✅ 60 tests totales
✅ 100% exitosos
```

**Resultado:** `CRUD_TEST_RESULTS.json`

---

## 📚 Documentación Completa

```
📄 INDEX.md
├─ Tabla de contenidos central
├─ Arquitectura completa
├─ Todos los módulos
├─ FAQ y troubleshooting
└─ ✅ LEE ESTO PRIMERO

📄 CUSTOM_CRUD_SUPERLIKES.md
├─ Explicación CP vs CEN
├─ Implementación de superlikes
├─ Casos de uso
└─ Integración Stripe

📄 ARCHITECTURE.md (próximo)
├─ Clean Architecture
├─ DDD explicado
├─ Patrón Repository
└─ Unit of Work

📄 QUICK_START.md (ESTE ARCHIVO)
├─ Guía rápida
└─ Ejemplos prácticos
```

---

## 🎯 Tareas Comunes

### ¿Cómo dar un superlike?

```csharp
var cp = new SuperlikeCP(...);
cp.Superlike(emisorId, receptorId);
```

### ¿Cómo comprar superlikes?

```csharp
cp.ComprarSuperlikes(usuarioId, cantidad);
```

### ¿Cómo hacer que dos usuarios se matcheen?

```csharp
// Usuario A da like
var matchCP = new IniciarMatchCP(...);
var match = matchCP.Iniciar(A, B);

// Usuario B acepta
var correspondCP = new CorresponderMatchCP(...);
correspondCP.Corresponder(B, A);

// Resultado: Match mutuo
```

### ¿Cómo crear una nueva funcionalidad?

```
1. Crear EN (Entidad) en Domain/EN/
2. Crear CEN (Lógica) en Domain/CEN/
3. Crear Repository en Infrastructure/Repositories/
4. Crear CP (Caso de uso) si es complejo en Domain/CP/
5. Crear Controller para API
```

---

## ⚡ Conceptos Clave Rápidos

| Término | Qué es | Ejemplo |
|---|---|---|
| **EN** | Entidad de dominio | Usuario, Match |
| **CEN** | Lógica de negocio | UsuarioCEN.Crear() |
| **CP** | Orquestación/Caso de uso | SuperlikeCP.Superlike() |
| **Repo** | Acceso a datos | IUsuarioRepository |
| **UoW** | Transacciones atómicas | IUnitOfWork.SaveChanges() |

---

## 🔐 Reglas Importantes

```
✅ DO:
├─ Usar CEN para lógica simple
├─ Usar CP para orquestación
├─ Validar siempre antes de modificar
├─ Usar UoW para transacciones
└─ Inyectar dependencias

❌ DON'T:
├─ Lógica de negocio en EN
├─ Acceso directo a BD desde Controller
├─ Modificar sin validar
├─ Transacciones sin UoW
└─ Métodos públicos sin error handling
```

---

## 📊 Flujos Principales Resumidos

### Flujo 1: Like Normal → Notificación

```
Usuario A Click Like (UI)
    ↓
POST /api/match/dar/{receptorId}
    ↓
IniciarMatchCP.Iniciar(A, B)
    ↓
✓ Validar (ambos existen, no baneados, no match previo)
    ↓
✓ Crear Match
    ↓
✓ B.LikesRecibidos++
    ↓
✓ Notificación a B
    ↓
Usuario B recibe notificación: "¡Usuario A te dio like!"
```

### Flujo 2: Superlike → Doble Contador

```
Usuario Premium A Click Superlike (UI)
    ↓
POST /api/superlike/dar/{receptorId}
    ↓
SuperlikeCP.Superlike(A, B)
    ↓
✓ Validar Premium + SuperlikesDisponibles > 0
    ↓
✓ Crear Match (EsSuperlike=true)
    ↓
✓ A.SuperlikesDisponibles--
    ↓
✓ B.LikesRecibidos += 2 ⭐
    ↓
✓ Notificación especial a B
    ↓
Usuario B recibe: "⭐ ¡Usuario A te envió SUPERLIKE! ⭐"
```

### Flujo 3: Corresponder → Match Mutuo

```
Usuario B Click Aceptar (UI)
    ↓
POST /api/match/corresponder/{matchId}
    ↓
CorresponderMatchCP.Corresponder(B, A)
    ↓
✓ Validar que existe match pendiente
    ↓
✓ Match.LikeReceptor = true
    ↓
✓ Match.FechaMatch = now ← MUTUO
    ↓
✓ A.NumMatchs++, B.NumMatchs++
    ↓
✓ Notificaciones a ambos
    ↓
AMBOS reciben: "¡MATCH! 💑"
```

---

## 💰 Monetización Rápida

```
Usuario Gratuito:
├─ $0/mes
├─ 0 superlikes
└─ Like = +1 punto

Usuario Premium:
├─ $9.99/mes
├─ 10 superlikes/mes
├─ Like = +1 punto
└─ Superlike = +2 puntos

Compra In-App:
├─ 5 superlikes: $0.99
├─ 15 superlikes: $2.49
└─ 30 superlikes: $4.99

Ingresos (1,000 usuarios):
├─ 100 Premium × $9.99 × 12 = $11,988/año
├─ 50 compras × $3 × 12 = $1,800/año
└─ TOTAL: $13,788/año
```

---

## 🚨 Troubleshooting

### "Build falla"
```
→ dotnet clean
→ dotnet restore
→ dotnet build
```

### "Errores en tests"
```
→ Verificar SQL Server está corriendo
→ Verificar connection string
→ Ejecutar: dotnet run --project InitializeDb
```

### "¿Por qué CP y CEN?"
```
→ Leer: CUSTOM_CRUD_SUPERLIKES.md sección "CP vs CEN"
→ Resumen: CEN=simple, CP=orquesta
```

### "¿Cómo agrego funcionalidad?"
```
→ 1. Crear EN (entidad)
→ 2. Crear CEN (lógica)
→ 3. Crear Repo
→ 4. Crear CP (si es complejo)
→ 5. Crear Controller
```

---

## 📞 Contacto & Links

- **GitHub:** https://github.com/TaponDeBotella/DSM-SpeedMatch
- **Docs:** Ver INDEX.md
- **Issues:** Reportar en GitHub

---

## ✅ Checklist: "Estoy Listo"

- ✅ Entiendo Clean Architecture (CP vs CEN)
- ✅ Ejecuté los tests (60/60 ok)
- ✅ Leí INDEX.md
- ✅ Entiendo monetización (Premium vs Basic)
- ✅ Sé dónde están los archivos importantes

**🎉 ¡Listo para empezar!**

---

**Versión:** 1.0  
**Status:** ✅ Ready to Go  
**Tiempo de lectura:** 10 minutos
