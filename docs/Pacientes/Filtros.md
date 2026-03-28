# Instrucciones para configurar ambos filtros

Dentro de bruno o cualquier cliente http se deben de hacer dos peticiones de tipo POST ya que al servidor se le deben de crear 2 search params personalizados.

Esto unicamente debe de hacerse una sola vez.

---

## 1. Crear Search Parameter para el **valor de la identificación**

En la parte de la URL debería ser así:

**Método**: `POST`  
**URL**: `http://localhost:Puerto/fhir/SearchParameter`

(El Puerto es el que aparece en la consola de Aspire al lado del recurso hapifhir, en mi caso aparece http://localhost:43963)

Y en Headers así:

### Headers
| Key           | Value                  |
|---------------|------------------------|
| `Content-Type` | `application/fhir+json` |

El cuerpo JSON así:

### Body (JSON)
```json
{
  "resourceType": "SearchParameter",
  "id": "patient-identifier-value-string",
  "url": "http://localhost:8080/fhir/SearchParameter/Patient/identifier-value",
  "name": "PatientIdentifierValueString",
  "status": "active",
  "code": "identifier-value",
  "base": [ "Patient" ],
  "type": "string",
  "expression": "Patient.identifier.value",
  "description": "Permite búsqueda por prefijo en el valor de los identificadores del paciente."
}
```

Hacen la petición y ya con eso ya tendrían funcionando el filtro de valor de la identificación

## 2. Crear Search Parameter para el **tipo de la identificación**

En la parte de la URL debería ser así:

**Método**: `POST`  
**URL**: `http://localhost:Puerto/fhir/SearchParameter` (la misma de antes)

(El Puerto es el que aparece en la consola de Aspire al lado del recurso hapifhir, en mi caso aparece http://localhost:43963, es el mismo puerto de antes)

Y en Headers así:

### Headers
| Key           | Value                  |
|---------------|------------------------|
| `Content-Type` | `application/fhir+json` |

El cuerpo JSON así:

### Body (JSON)
```json
{
  "resourceType": "SearchParameter",
  "id": "patient-identifier-type",
  "url": "http://localhost:8080/fhir/SearchParameter/Patient-identifier-type",
  "name": "PatientIdentifierType",
  "status": "active",
  "code": "identifier-type",
  "base": ["Patient"],
  "type": "token",
  "expression": "Patient.identifier.type.coding.code",
  "xpath": "f:Patient/f:identifier/f:type/f:coding/f:code",
  "xpathUsage": "normal"
}
```

Hacen la petición y ya con eso se tiene solucionado el filtro del tipo de la identificación

Ya con eso no se debe de tocar nada mas y ambos filtros deben de funcionar correctamente.