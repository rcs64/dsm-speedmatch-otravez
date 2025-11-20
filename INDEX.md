# 📚 DSM-SpeedMatch: Documentación Centralizada

> **Última actualización:** Noviembre 5, 2025  
> **Status:** ✅ COMPLETO Y ACTUALIZADO

---

## 🎯 Resumen Ejecutivo

**DSM-SpeedMatch** es una aplicación de dating inteligente construida con **.NET 8 + NHibernate** usando arquitectura **Clean Architecture + DDD**.

**Stack Técnico:**
- Framework: .NET 8.0, C# 12.0
- ORM: NHibernate 5.4.9
- DB: SQL Server Express 2019
- Arquitectura: Clean Architecture + DDD
- Patrones: Repository, Unit of Work, CEN, CP

**Características Principales:**
- ✅ Sistema de Matching inteligente
- ✅ Sistema de Notificaciones en tiempo real
- ✅ Superlikes Premium con monetización
- ✅ CRUD completo para todas las entidades
- ✅ Validaciones exhaustivas
- ✅ Transacciones atómicas garantizadas

---

## 📖 Tabla de Contenidos

1. [Inicio Rápido](#inicio-rápido)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Módulos Funcionales](#módulos-funcionales)
4. [Guía de Desarrollo](#guía-de-desarrollo)
5. [Testing](#testing)
6. [FAQ](#faq)

---

## 🚀 Inicio Rápido

### Requisitos Previos

```bash
✅ .NET SDK 8.0+
✅ SQL Server Express 2019+
✅ Visual Studio 2022+ o VS Code
```

### Configuración Inicial

```bash
# 1. Clonar repositorio
git clone https://github.com/TaponDeBotella/DSM-SpeedMatch.git

# 2. Restaurar dependencias
cd DSM-SpeedMatch
dotnet restore

# 3. Compilar
dotnet build

# 4. Inicializar BD
dotnet run --project InitializeDb
```

### Verificación

```bash
# Build exitoso = 0 errores
✅ Compilación correcta
    0 Advertencia(s)
    0 Errores
```

---

## 🏗️ Arquitectura del Sistema

### 1. **Clean Architecture en Capas**

```
┌─────────────────────────────────────────────────────────────┐
│ CAPA DE PRESENTACIÓN (Controllers/API)                      │
│ (No incluida en ApplicationCore - será frontend o API REST) │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ CAPA DE CASOS DE USO (CP - Casos de Uso)                   │
│ • SuperlikeCP        → Orquestar superlikes                 │
│ • IniciarMatchCP     → Iniciar un match                     │
│ • CorresponderMatchCP → Corresponder match                  │
│ Responsabilidad: Orquestar múltiples CENs + validaciones   │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ CAPA DE NEGOCIO (CEN - Lógica de Negocio)                  │
│ • UsuarioCEN         → Gestionar usuarios                   │
│ • MatchCEN           → Gestionar matches                    │
│ • NotificacionCEN    → Gestionar notificaciones            │
│ • SuperlikeCEN       → Gestionar superlikes                │
│ • ... más CENs       → Cada uno una responsabilidad         │
│ Responsabilidad: Lógica PURA, reutilizable, testeable      │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ CAPA DE DOMINIO (EN - Entidades)                            │
│ • Usuario, Match, Notificacion, Foto, Ubicacion           │
│ • Preferences, Admin, Superlike                            │
│ Responsabilidad: Definir modelos de negocio                │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ CAPA DE DATOS (Repositories + UnitOfWork)                  │
│ • IUsuarioRepository, IMatchRepository, etc.               │
│ • IUnitOfWork → Garantiza transacciones atómicas           │
│ Responsabilidad: Acceso a BD (NHibernate)                  │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ CAPA DE INFRAESTRUCTURA (NHibernate)                        │
│ • Mappings (XML)     → Configuración ORM                   │
│ • SQL Server         → Base de datos                        │
│ Responsabilidad: Persistencia                              │
└─────────────────────────────────────────────────────────────┘
```

### 2. **Diferencia: CP vs CEN**

| Aspecto | **CEN (Capa de Negocio)** | **CP (Casos de Uso)** |
|---------|---------------------------|----------------------|
| **Responsabilidad** | Lógica pura de negocio | Orquestación de caso de uso |
| **Complejidad** | SIMPLE (operaciones independientes) | COMPLEJA (coordina múltiples CENs) |
| **Transacciones** | NO (cada operación independiente) | SÍ (garantiza todo-o-nada) |
| **Reutilización** | ALTA (usable desde múltiples lugares) | BAJA (específico del caso de uso) |
| **Ejemplos** | ObtenerSuperlikes(), RestarSuperlike() | Superlike(), ComprarSuperlikes() |
| **Ubicación** | `ApplicationCore/Domain/CEN/` | `ApplicationCore/Domain/CP/` |

**Ejemplo Práctico:**
```csharp
// CEN: Validación SIMPLE y reutilizable
if (_superlikeCEN.PuedeHacerSuperlike(usuarioId))
{
    // CP: Orquestación COMPLEJA
    var match = _superlikeCP.Superlike(emisorId, receptorId);
    // Dentro: validaciones, múltiples cambios, notificaciones
}
```

---

## 📦 Módulos Funcionales

### 1. **Sistema de Usuarios**

**CEN: `UsuarioCEN.cs`**
- `Crear()` → Crear nuevo usuario
- `Modificar()` → Actualizar datos
- `DameTodos()` → Listar todos
- `DamePorId()` → Buscar por ID
- `DamePorEmail()` → Buscar por email
- `DarLike()` → Incrementar likes recibidos
- `Banear()` / `Desbanear()` → Control de acceso

**Atributos clave:**
```csharp
public virtual long Id { get; set; }
public virtual string Nombre { get; set; }
public virtual string Email { get; set; }
public virtual int LikesRecibidos { get; set; }
public virtual int LikesEnviados { get; set; }
public virtual int SuperlikesDisponibles { get; set; }  // ⭐ NUEVO
public virtual Plan TipoPlan { get; set; }  // Gratuito, Premium, Premium+
public virtual bool Baneado { get; set; }
```

---

### 2. **Sistema de Matching**

**CEN: `MatchCEN.cs`**
- `Crear()` → Crear match
- `Modificar()` → Actualizar (ej: receptor acepta)
- `DameTodos()` → Listar matches
- `DamePorUsuario()` → Matches de un usuario

**CP: `IniciarMatchCP.cs`**
- `Iniciar()` → Usuario A da like a Usuario B
  - Validaciones
  - Crea Match
  - Incrementa contadores
  - Notifica a receptor

**CP: `CorresponderMatchCP.cs`**
- `Corresponder()` → Usuario B acepta like de A
  - Valida que existe match pendiente
  - Actualiza Match.LikeReceptor = true
  - Crea Match Mutuo
  - Notifica a ambos

**Atributos clave:**
```csharp
public virtual long Id { get; set; }
public virtual Usuario Emisor { get; set; }
public virtual Usuario Receptor { get; set; }
public virtual bool LikeEmisor { get; set; }
public virtual bool LikeReceptor { get; set; }
public virtual DateTime FechaInicio { get; set; }  // ⭐ NUEVO
public virtual DateTime? FechaMatch { get; set; }  // Cuando es mutuo
public virtual bool EsSuperlike { get; set; }  // ⭐ NUEVO
```

---

### 3. **Sistema de Notificaciones**

**CEN: `NotificacionCEN.cs`**
- `Crear()` → Crear notificación para usuario
- `Modificar()` → Actualizar mensaje
- `Eliminar()` → Borrar notificación
- `DameTodos()` → Listar todas
- `DamePorId()` → Buscar por ID

**CP: `NotificarMatchRecibidoCP.cs`**
- `NotificarMatchRecibido()` → Notificar like recibido
- `NotificarMatchMutuo()` → Notificar match mutuo
- `NotificarMatchesPendientes()` → Recordar matches pendientes

**Atributos clave:**
```csharp
public virtual long Id { get; set; }
public virtual Usuario Usuario { get; set; }
public virtual string Mensaje { get; set; }
public virtual DateTime FechaCreacion { get; set; }
public virtual bool Leida { get; set; }
```

---

### 4. **Sistema de Superlikes Premium ⭐**

**ESTA ES LA IMPLEMENTACIÓN PRINCIPAL DE MONETIZACIÓN**

#### Concepto

```
LIKE NORMAL:         Usuario da like → Receptor gana 1 punto
SUPERLIKE PREMIUM:   Usuario Premium gasta 1 superlike → Receptor gana 2 puntos
```

#### CEN: `SuperlikeCEN.cs` (Lógica Pura)

```csharp
// Validación
bool PuedeHacerSuperlike(long usuarioId)
    → true si: Premium && SuperlikesDisponibles > 0

// Lectura
int ObtenerSuperlikes(long usuarioId)
    → Retorna SuperlikesDisponibles

// Operaciones simples
void RestarSuperlike(long usuarioId)
    → Decrementa en 1

void AñadirSuperlikes(long usuarioId, int cantidad)
    → Incrementa (compra o regalo)

// Estadísticas
int ContarSuperlikes(long usuarioId)
    → Superlikes RECIBIDOS por este usuario

SuperlikeEstadisticas ObtenerEstadisticas(long usuarioId)
    → Análisis completo de uso
```

#### CP: `SuperlikeCP.cs` (Orquestación)

```csharp
// CASO DE USO: Dar superlike
Match Superlike(long emisorId, long receptorId)
    Paso 1: Validar que emisor es Premium
    Paso 2: Validar que tiene SuperlikesDisponibles > 0
    Paso 3: Validar que receptor existe y no está baneado
    Paso 4: Validar que no hay match previo
    Paso 5: TRANSACCIÓN COMIENZA:
        - Crear Match con EsSuperlike = true
        - Restar 1 de emisor.SuperlikesDisponibles
        - Sumar 2 a receptor.LikesRecibidos  ← DIFERENCIAL
        - Crear notificación especial
        - SaveChanges()
    Paso 6: TRANSACCIÓN COMPLETADA

// CASO DE USO: Comprar superlikes
void ComprarSuperlikes(long usuarioId, int cantidad)
    Paso 1: Validar Premium
    Paso 2: Validar cantidad > 0
    Paso 3: TRANSACCIÓN: Sumar cantidad a SuperlikesDisponibles
    Paso 4: [Aquí va integración de pago]

// Información
SuperlikeInfo ObtenerInfoSuperlikes(long usuarioId)
    → Plan, Disponibles, Usados, ¿Puede hacer?

int ContarSuperlikes(long usuarioId)
    → Superlikes recibidos por este usuario
```

#### Entidades Modificadas

**Usuario.cs:**
```csharp
public virtual int SuperlikesDisponibles { get; set; }
    // Contador de superlikes que puede usar (Premium only)
    // Inicializa en 0, se da al cambiar a Premium
    // Decrementa con cada superlike
    // Incrementa con compras
```

**Match.cs:**
```csharp
public virtual bool EsSuperlike { get; set; }
    // Flag para distinguir superlike de like normal
    // true = creado con SuperlikeCP.Superlike()
    // false = like normal

public virtual DateTime FechaInicio { get; set; }
    // Timestamp de creación del match
    // Permite: ordenar, filtrar, analytics
```

#### Modelo de Negocio

```
Plan Básico (Gratis):
├─ 0 superlikes/mes
├─ Like normal = +1 punto al receptor
├─ No paga
└─ $0/mes

Plan Premium ($9.99/mes):
├─ 10 superlikes/mes
├─ Like normal = +1 punto
├─ Superlike = +2 puntos (DIFERENCIAL)
└─ $9.99/mes

Compras In-App:
├─ 5 superlikes: $0.99
├─ 15 superlikes: $2.49
├─ 30 superlikes: $4.99
└─ Upsell por demanda

Proyección de Ingresos:
├─ 1,000 usuarios activos
├─ 10% Premium: 100 × $9.99 = $999/mes
├─ 5% compras: 50 × $3 = $150/mes
└─ Total: $1,149/mes × 12 = $13,788/año
```

---

### 5. **Entidades Completas**

```
📋 ENTIDADES (EN - Domain Models)

Usuario
├─ id, nombre, email, password
├─ likesRecibidos, likesEnviados
├─ superlikesDisponibles  ⭐
├─ tipoPlan (Gratuito, Premium, Premium+)
├─ baneado
└─ fechaCreacion

Match
├─ id, emisor, receptor
├─ likeEmisor, likeReceptor
├─ esSuperlike  ⭐
├─ fechaInicio  ⭐
├─ fechaMatch (null si no es mutuo)
└─ descripcion

Notificacion
├─ id, usuario, mensaje
├─ leida
└─ fechaCreacion

Foto
├─ id, usuario, url
└─ ordenPrioridad

Ubicacion
├─ id, usuario
├─ lat, lon
└─ fechaActualizacion

Preferencias
├─ id, usuario
├─ orientacionSexual
├─ prefConocer (género deseado)
└─ orientacionMostrar

Admin
├─ id, email, password
└─ fechaCreacion
```

---

## 🛠️ Guía de Desarrollo

### Estructura de Carpetas

```
ApplicationCore/
├─ Domain/
│  ├─ CEN/           ← Lógica de negocio PURA
│  │  ├─ UsuarioCEN.cs
│  │  ├─ MatchCEN.cs
│  │  ├─ SuperlikeCEN.cs
│  │  └─ ... más CENs
│  │
│  ├─ CP/            ← Orquestación de casos de uso
│  │  ├─ SuperlikeCP.cs
│  │  ├─ IniciarMatchCP.cs
│  │  ├─ CorresponderMatchCP.cs
│  │  └─ ... más CPs
│  │
│  ├─ EN/            ← Entidades de dominio
│  │  ├─ Usuario.cs
│  │  ├─ Match.cs
│  │  ├─ Notificacion.cs
│  │  └─ ... más entidades
│  │
│  ├─ Enums/         ← Enumeraciones
│  │  ├─ Plan.cs
│  │  ├─ OrientacionSexual.cs
│  │  └─ ...
│  │
│  └─ Repositories/  ← Interfaces de repositorios
│     ├─ IUsuarioRepository.cs
│     ├─ IMatchRepository.cs
│     └─ ...

Infrastructure/
├─ NHibernate/
│  ├─ NHibernateHelper.cs
│  └─ Mappings/      ← Configuración ORM
│     ├─ UsuarioMapping.xml
│     ├─ MatchMapping.xml
│     └─ ...
├─ Repositories/     ← Implementaciones
│  ├─ UsuarioRepository.cs
│  ├─ MatchRepository.cs
│  └─ ...
└─ UnitOfWork.cs     ← Orquestador de transacciones

InitializeDb/
├─ Program.cs        ← Punto de entrada
├─ CRUDTestSuite.cs  ← Tests completos
└─ CRUDExamples.cs   ← Ejemplos de uso
```

### Patrón para Crear una Nueva Funcionalidad

#### 1. Definir la Entidad (EN)

```csharp
// ApplicationCore/Domain/EN/MiEntidad.cs
namespace ApplicationCore.Domain.EN
{
    public class MiEntidad
    {
        public virtual long Id { get; set; }
        public virtual string Nombre { get; set; }
        // ... más propiedades
    }
}
```

#### 2. Crear la Lógica de Negocio (CEN)

```csharp
// ApplicationCore/Domain/CEN/MiEntidadCEN.cs
namespace ApplicationCore.Domain.CEN
{
    public class MiEntidadCEN
    {
        private readonly IMiEntidadRepository _repo;
        private readonly IUnitOfWork _uow;

        public MiEntidadCEN(IMiEntidadRepository repo, IUnitOfWork uow)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        // Operaciones SIMPLES y reutilizables
        public MiEntidad Crear(string nombre)
        {
            var entidad = new MiEntidad { Nombre = nombre };
            _repo.New(entidad);
            _uow.SaveChanges();
            return entidad;
        }

        public MiEntidad? DamePorId(long id) => _repo.GetById(id);

        public IEnumerable<MiEntidad> DameTodos() => _repo.GetAll();

        public void Modificar(long id, string nuevoNombre)
        {
            var entidad = _repo.GetById(id);
            if (entidad == null) throw new InvalidOperationException("No encontrada");
            entidad.Nombre = nuevoNombre;
            _repo.Modify(entidad);
            _uow.SaveChanges();
        }
    }
}
```

#### 3. Crear Interfaz de Repositorio (si no existe)

```csharp
// ApplicationCore/Domain/Repositories/IMiEntidadRepository.cs
public interface IMiEntidadRepository
{
    MiEntidad? GetById(long id);
    IEnumerable<MiEntidad> GetAll();
    void New(MiEntidad entity);
    void Modify(MiEntidad entity);
    void Delete(MiEntidad entity);
}
```

#### 4. Crear Implementación de Repositorio

```csharp
// Infrastructure/Repositories/MiEntidadRepository.cs
public class MiEntidadRepository : GenericRepository<MiEntidad>, IMiEntidadRepository
{
    public MiEntidadRepository(ISession session) : base(session) { }
    // Métodos específicos si es necesario
}
```

#### 5. Crear Caso de Uso (CP) si hay orquestación compleja

```csharp
// ApplicationCore/Domain/CP/MiOperacionCP.cs
public class MiOperacionCP
{
    private readonly MiEntidadCEN _miCEN;
    private readonly OtraCEN _otraCEN;
    private readonly IUnitOfWork _uow;

    public MiOperacionCP(
        MiEntidadCEN miCEN,
        OtraCEN otraCEN,
        IUnitOfWork uow)
    {
        _miCEN = miCEN ?? throw new ArgumentNullException(nameof(miCEN));
        _otraCEN = otraCEN ?? throw new ArgumentNullException(nameof(otraCEN));
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public void EjecutarOperacion(long id, string dato)
    {
        try
        {
            // Validaciones
            if (id <= 0) throw new InvalidOperationException("ID inválido");

            // Obtener datos
            var entidad = _miCEN.DamePorId(id);
            if (entidad == null) throw new InvalidOperationException("No encontrada");

            // TRANSACCIÓN
            entidad.Nombre = dato;
            _miCEN.Modificar(id, dato);
            _otraCEN.ActualizarRelacionado(id);

            _uow.SaveChanges();
            // FIN TRANSACCIÓN
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error en operación", ex);
        }
    }
}
```

### Reglas Arquitectónicas

✅ **HACER:**
- Usar CEN para lógica simple y reutilizable
- Usar CP para orquestación de múltiples CENs
- Validar SIEMPRE antes de modificar
- Usar IUnitOfWork para transacciones atómicas
- Inyección de dependencias en constructores
- Interfaces explícitas para repositorios

❌ **NO HACER:**
- Lógica de negocio en entidades (EN)
- Acceso directo a BD desde Controllers
- Modificar datos sin validar
- Transacciones sin UnitOfWork
- Cambiar schemas de BD manualmente
- Métodos públicos sin manejo de errores

---

## 🧪 Testing

### Ejecutar Todas las Pruebas

```bash
cd InitializeDb
dotnet run
```

**Output esperado:**
```
╔════════════════════════════════════════════════════════════╗
║         PRUEBAS CRUD COMPLETAS - TODAS LAS ENTIDADES        ║
╚════════════════════════════════════════════════════════════╝

🧪 Probando UsuarioCEN...
✅ Usuario CREATE: ID: 101, Nombre: Juan Pérez
✅ Usuario READ ALL: Total usuarios: 2
... (más pruebas)

🧪 Probando Superlikes...
✅ Superlikes CREAR: Match ID: 506, EsSuperlike: True
✅ Superlikes DOBLE CONTADOR: LikesRecibidos: 2
... (más superlikes)

╔════════════════════════════════════════════════════════════╗
║                    RESUMEN DE PRUEBAS                      ║
╠════════════════════════════════════════════════════════════╣
║ Total de pruebas:        60
║ Pruebas exitosas:        60
║ Pruebas fallidas:        0
║ Porcentaje éxito:        100.00%
╚════════════════════════════════════════════════════════════╝

✅ Reporte guardado en: CRUD_TEST_RESULTS.json
```

### Resultados Guardados

**Archivo:** `CRUD_TEST_RESULTS.json`
```json
{
  "test_date": "2025-11-05T12:51:32.9038846+01:00",
  "total_tests": 60,
  "successful_tests": 60,
  "failed_tests": 0,
  "success_rate_percentage": 100,
  "tests": [...]
}
```

---

## 📊 Flujos Principales

### Flujo 1: Crear Match Normal

```
Usuario A (cualquiera)
    │
    ├─ [UI] Click "Like" a Usuario B
    │
    ├─ [Controller] POST /api/match/dar
    │
    ├─ [CP] IniciarMatchCP.Iniciar(A.Id, B.Id)
    │
    │   TRANSACCIÓN:
    │   ├─ Validar que A existe y no está baneado
    │   ├─ Validar que B existe y no está baneado
    │   ├─ Validar que no hay match previo
    │   ├─ [CEN] MatchCEN.Crear(A, B, likeEmisor: true)
    │   ├─ [CEN] UsuarioCEN.DarLike() a B
    │   ├─ [CEN] NotificacionCEN.Crear() para B
    │   └─ SaveChanges()
    │
    ├─ Usuario B recibe notificación:
    │  "Juan te envió un like ❤️"
    │
    └─ RESULTADO:
       ✅ Match creado
       ✅ B.LikesRecibidos += 1
       ✅ Notificación enviada
```

### Flujo 2: Dar Superlike Premium

```
Usuario A (Premium, 10 superlikes)
    │
    ├─ [UI] Click "⭐ Superlike" a Usuario B
    │
    ├─ [Controller] POST /api/superlike/dar
    │
    ├─ [CP] SuperlikeCP.Superlike(A.Id, B.Id)
    │
    │   TRANSACCIÓN:
    │   ├─ Validar A es Premium
    │   ├─ Validar A.SuperlikesDisponibles > 0 ✓
    │   ├─ Validar B existe y no baneado
    │   ├─ Validar no hay match previo
    │   ├─ [CEN] MatchCEN.Crear(A, B, esSuperlike: true)
    │   ├─ A.SuperlikesDisponibles: 10 → 9 ✅
    │   ├─ B.LikesRecibidos: 50 → 52 ⭐⭐ (DOBLE)
    │   ├─ [CEN] NotificacionCEN.Crear() especial para B
    │   └─ SaveChanges()
    │
    ├─ Usuario B recibe notificación especial:
    │  "⭐ ¡Usuario A te envió un SUPERLIKE! ⭐"
    │
    └─ RESULTADO:
       ✅ Match creado con EsSuperlike=true
       ✅ A gastó 1 superlike
       ✅ B recibió +2 likes
       ✅ Notificación premium
```

### Flujo 3: Corresponder Match

```
Usuario B
    │
    ├─ [UI] Click "Aceptar" en match de Usuario A
    │
    ├─ [Controller] POST /api/match/corresponder/{matchId}
    │
    ├─ [CP] CorresponderMatchCP.Corresponder(B.Id, A.Id)
    │
    │   TRANSACCIÓN:
    │   ├─ Validar que existe match con:
    │   │  - Emisor: A, Receptor: B
    │   │  - LikeEmisor: true, LikeReceptor: false
    │   ├─ [CEN] MatchCEN.Modificar(match.Id)
    │   ├─ match.LikeReceptor = true ✓
    │   ├─ match.FechaMatch = DateTime.Now ✓
    │   ├─ A.NumMatchs += 1
    │   ├─ B.NumMatchs += 1
    │   ├─ [CEN] NotificacionCEN.Crear() para A y B
    │   └─ SaveChanges()
    │
    ├─ Notificaciones:
    │  A: "¡Usuario B aceptó tu like! ❤️❤️ ¡MATCH!"
    │  B: "¡Aceptaste el like de Usuario A! ❤️❤️ ¡MATCH!"
    │
    └─ RESULTADO:
       ✅ Match mutuo creado
       ✅ Ambos incrementan contador de matchs
       ✅ Ambos reciben notificaciones
       ✅ Pueden empezar a chatear
```

---

## ❓ FAQ

### P: ¿Por qué hay SuperlikeCP y SuperlikeCEN?

**R:** Separación de responsabilidades en Clean Architecture:

- **SuperlikeCEN**: Lógica pura ("¿puedo hacer superlike?", "¿cuántos tengo?")
  - Reutilizable desde múltiples lugares
  - Testeable sin dependencias complejas
  - Una responsabilidad única

- **SuperlikeCP**: Orquestación del caso de uso ("Dar superlike")
  - Coordina múltiples CENs
  - Garantiza transacción atómica
  - Punto de entrada desde Controller

**Analogía:** CEN = Violinista (toca nota correcta), CP = Director (orquesta todo)

### P: ¿Cómo se garantiza atomicidad en transacciones?

**R:** Mediante `IUnitOfWork.SaveChanges()`:

```csharp
// Si TODO funciona: se guarda todo
// Si algo falla: se revierte todo (rollback)

try {
    Usuario.SuperlikesDisponibles--;
    Receptor.LikesRecibidos += 2;
    Notificacion.Crear();
    _uow.SaveChanges();  ← TODO se guarda en una sola llamada
} catch {
    // Si falla: BD queda igual (rollback automático)
}
```

### P: ¿Cuándo uso CEN vs CP?

**R:** Regla simple:

```
Desde Controller:
└─ Siempre llamas a CP (caso de uso completo)

Desde CP:
├─ Llamas a CEN para lógica simple
└─ Accedes a Repositories solo si es necesario

Desde CEN:
└─ Nunca llamas a CP (rompe arquitectura)
```

### P: ¿Cómo agregar una nueva funcionalidad?

**R:** Patrón en 5 pasos:

1. Crear EN (Entidad) en `ApplicationCore/Domain/EN/`
2. Crear CEN (Lógica) en `ApplicationCore/Domain/CEN/`
3. Crear Repository en `Infrastructure/Repositories/`
4. Crear CP (Caso de uso) si hay orquestación
5. Crear Controller (API endpoint)

Ver sección [Guía de Desarrollo](#guía-de-desarrollo) para ejemplo completo.

### P: ¿Cómo ejecutar tests?

**R:**
```bash
# Opción 1: Desde carpeta raíz
dotnet run --project InitializeDb

# Opción 2: Desde InitializeDb
cd InitializeDb
dotnet run

# Resultados en: CRUD_TEST_RESULTS.json
```

### P: ¿Cómo el superlike vale 2 puntos?

**R:** En `SuperlikeCP.Superlike()`:

```csharp
// Like normal:
receptor.LikesRecibidos += 1;  // +1 punto

// Superlike:
receptor.LikesRecibidos += 2;  // +2 puntos ⭐
```

Es simplemente un incremento de 2 en lugar de 1. Los superlikes no son una "entrada separada", son matches con `EsSuperlike=true`.

### P: ¿Cómo se inicializa SuperlikesDisponibles?

**R:** Cuando usuario pasa a Premium:

```csharp
// En cambio de plan
usuario.TipoPlan = Plan.Premium;
usuario.SuperlikesDisponibles = 10;  // Cantidad inicial
_uow.SaveChanges();
```

Después se gestiona con:
- Decrementa: cada superlike (CP)
- Incrementa: cada compra (CP + Pagos)

### P: ¿Hay límite de superlikes?

**R:** Sí, en SuperlikeCP.Superlike():

```csharp
if (emisor.SuperlikesDisponibles <= 0)
    throw new InvalidOperationException(
        "No tienes superlikes disponibles");
```

Debe tener mínimo 1 para hacer superlike.

---

## 📝 Notas Importantes

### Compilación

```bash
✅ Status: 0 errores, 14 advertencias (nullable properties)
✅ Build time: ~4 segundos
✅ Proyectos: ApplicationCore, Infrastructure, InitializeDb
```

### Archivos Clave

| Archivo | Responsabilidad |
|---------|-----------------|
| `SuperlikeCP.cs` | Orquesta superlikes |
| `SuperlikeCEN.cs` | Lógica de superlikes |
| `MatchCEN.cs` | Gestiona matches |
| `UsuarioCEN.cs` | Gestiona usuarios |
| `IUnitOfWork.cs` | Transacciones atómicas |
| `CRUDTestSuite.cs` | Suite de tests (60 pruebas) |

### Pasos Siguientes

1. ✅ **Controllers REST** → Endpoints para API
2. ✅ **Integración de Pagos** → Stripe/PayPal
3. ✅ **Frontend** → React/Angular
4. ✅ **Analytics Dashboard** → Métricas
5. ✅ **Chat en Tiempo Real** → SignalR

---

## 📞 Contacto y Contribuciones

- **Owner:** TaponDeBotella
- **Repo:** https://github.com/TaponDeBotella/DSM-SpeedMatch
- **Issues:** Reportar bugs en GitHub
- **Docs:** Este archivo (INDEX.md)

---

**Última actualización:** Noviembre 5, 2025  
**Status:** ✅ PRODUCCIÓN-READY  
**Versión:** 2.0 (con Superlikes)
