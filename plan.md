# Implementation Plan: Role and Permission Models

## Overview
Create 4 model classes for the role and permission system. These classes will be independent entities without any database relationships or foreign key references, as relationships will be handled in the backend.

## Classes to Create

### 1. EmpRole (Associative Entity)
**File**: `Models/EmpRole.cs`
- **empId**: string (required)
- **roleId**: int (required)
- **systemId**: int (required)

### 2. Role
**File**: `Models/Role.cs` (Note: This will replace the existing Role.cs)
- **roleId**: int (required)
- **roleName**: string (required)
- **roleDescription**: string? (nullable)

### 3. RolePermission (Associative Entity)
**File**: `Models/RolePermission.cs`
- **roleId**: int (required)
- **permissionId**: int (required)
- **systemId**: int (required)

### 4. Permission
**File**: `Models/Permission.cs`
- **permissionId**: int (required)
- **permissionName**: string (required)
- **permissionDescription**: string? (nullable)
- **systemId**: int (required)

## Implementation Details

### Key Requirements:
1. **No Relationships**: Classes will not reference each other
2. **No Key Attributes**: No `[Key]` or other data annotation attributes
3. **No Navigation Properties**: No foreign key or relationship properties
4. **Namespace**: All classes will be in `THMY_API.Models` namespace
5. **Property Modifiers**: 
   - Required properties will use `required` keyword
   - Nullable properties will use `?` nullable modifier
   - String properties will use `string` type

### File Structure:
```
Models/
├── EmpRole.cs          (new)
├── Role.cs             (replace existing)
├── RolePermission.cs   (new)
└── Permission.cs       (new)
```

### Code Pattern:
Each class will follow this structure:
```csharp
namespace THMY_API.Models;

public class ClassName
{
    public required [type] propertyName { get; set; }
    public [type]? nullablePropertyName { get; set; }
}
```

## Notes
- The existing `Role.cs` will be replaced with the new specification
- User class is not needed as it will be called from another DLL
- All classes are independent entities for backend relationship handling
- No Entity Framework relationship configuration needed

## Next Steps
After approval, I will:
1. Create the 4 model classes following the specifications
2. Replace the existing Role.cs with the new structure
3. Ensure all properties match the exact requirements provided

