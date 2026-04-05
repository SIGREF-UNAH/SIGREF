import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
  vus: 1, //  solo 1 usuario, esto NO es carga
  iterations: 20, // número de pruebas
};

function generateIds(count) {
  let ids = [];
  for (let i = 0; i < count; i++) {
    ids.push(`id-${i}`);
  }
  return ids;
}

export default function () {
  // aumenta progresivamente
  let count = (__ITER + 1) * 50; // 50, 100, 150, 200...

  const ids = generateIds(count);
  const query = ids.map(id => `ids=${id}`).join('&');
  const url = `http://localhost:5226/api/Users/by-ids?${query}`;

  let res = http.get(url);

  console.log(`IDs: ${count} | Status: ${res.status} | URL length: ${url.length}`);

  sleep(1);
}