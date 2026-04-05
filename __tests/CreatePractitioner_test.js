import http from 'k6/http';
import { check, sleep } from 'k6';

// =============================================================================
// 1. CONFIGURACIÓN DEL ESCENARIO (STAGES)
// =============================================================================
export let options = {
  stages: [
    { duration: '30s', target: 10 }, // Sube a 10 usuarios concurrentes
    { duration: '1m', target: 20 },  // Mantiene 20 usuarios (presión media)
    { duration: '30s', target: 0 },  // Baja a 0 gradualmente
  ],
};

// =============================================================================
// 2. BANCO DE DATOS (Agrega más aquí para mayor variedad)
// =============================================================================
const names = ['Héctor', 'René', 'María', 'José', 'Carlos', 'Ana', 'Lucía', 'David', 'Sofía', 'Miguel'];
const surnames = ['Martínez', 'Vega', 'López', 'García', 'Rodríguez', 'Pérez', 'Sánchez', 'Gómez'];
const domains = ['gmail.com', 'hotmail.com', 'yahoo.com'];
const genders = ['male', 'female', 'other', 'unknown']; // Géneros permitidos por el Enum

// =============================================================================
// 3. AUTENTICACIÓN (Se ejecuta una vez antes de empezar el test)
// =============================================================================
export function setup() {
  const payload = {
    grant_type: 'password',
    client_id: 'frontend', 
    username: 'admin',      // <-- Usuario con rol admin/ti
    password: '123',        // <-- Contraseña del usuario
  };

  const res = http.post('http://localhost:8080/realms/sigref/protocol/openid-connect/token', payload);
  
  if (res.status !== 200) {
    throw new Error(`Error en Keycloak: ${res.status} - ${res.body}`);
  }

  return { token: res.json().access_token };
}

// =============================================================================
// 4. FLUJO PRINCIPAL (Lo que hace cada Usuario Virtual)
// =============================================================================
export default function (data) {
  // CONFIGURACIÓN DE URL Y HEADERS
  const url = 'http://localhost:5226/api/practitioner'; // -- Asegura que el puerto coincida con tu backend
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${data.token}`,
  };

  // GENERACIÓN DE DATOS ALEATORIOS
  const randomName = names[Math.floor(Math.random() * names.length)];
  const randomSurname = surnames[Math.floor(Math.random() * surnames.length)];
  const randomGender = genders[Math.floor(Math.random() * genders.length)];
  const randomDomain = domains[Math.floor(Math.random() * domains.length)];
  
  // Genera un email: nombre.apellido.numero@dominio.com
  const email = `${randomName.toLowerCase()}.${randomSurname.toLowerCase()}${Math.floor(Math.random() * 999)}@${randomDomain}`;
  
  // Genera un DNI ficticio único para evitar errores 400 por duplicados
  const dni = `0801-${Math.floor(1970 + Math.random() * 40)}-${Math.floor(Math.random() * 999999).toString().padStart(6, '0')}`;
  
  // Genera un teléfono de Honduras (8 o 9 dígitos aleatorios)
  const phone = `${[3, 8, 9][Math.floor(Math.random() * 3)]}${Math.floor(1000000 + Math.random() * 8999999)}`;

  // CONSTRUCCIÓN DEL BODY SEGÚN EL DTO DE SIGREF
  const body = JSON.stringify({
    identifier: [{
      use: "usual",
      system: "http://sigref.hn/identifiers/dni",
      value: dni
    }],
    active: true,
    name: [{
      use: "usual",
      text: `${randomName} ${randomSurname}`,
      family: randomSurname,
      given: [randomName],
      prefix: ["Lic."],
      suffix: []
    }],
    telecom: [
      {
        system: "phone",
        value: phone,
        use: "mobile",
        rank: 1
      },
      {
        system: "email",
        value: email,
        use: "work",
        rank: 2
      }
    ],
    gender: randomGender, // 'male', 'female', 'other', 'unknown'
    birthDate: "1990-01-01T00:00:00.000Z"
  });

  // EJECUCIÓN DE LA PETICIÓN
  const res = http.post(url, body, { headers });

  // VALIDACIÓN DE RESULTADOS
  const isCreated = check(res, {
    'Practitioner creado (201)': (r) => r.status === 201,
  });

  // Si algo sale mal, imprime el porqué en la consola de K6
  if (!isCreated) {
    console.error(`ERROR ${res.status}: ${res.body} | Payload enviado: ${body}`);
  }

  // Tiempo de espera entre peticiones por cada usuario (ajusta según necesites velocidad)
  sleep(0.5); 
}