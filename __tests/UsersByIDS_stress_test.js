import http from 'k6/http';
import { check, sleep } from 'k6';

// puedes cambiar esto al ejecutar: k6 run script.js -e IDS_COUNT=100
const IDS_COUNT = __ENV.IDS_COUNT ? parseInt(__ENV.IDS_COUNT) : 50;

export let options = {
  stages: [
    { duration: '30s', target: 20 }, // Sube de 0 a 20 VUs
    { duration: '1m', target: 50 },  // Sube de 20 a 50 VUs
    { duration: '20s', target: 0 },  // Baja a 0
  ],
};
function generateIds(count) {
  let ids = [];
  for (let i = 0; i < count; i++) {
    ids.push(`id-${i}`);
  }
  return ids;
}

export default function () {
  const ids = generateIds(IDS_COUNT);
  const query = ids.map(id => `ids=${id}`).join('&');
  const url = `http://localhost:5226/api/Users/by-ids?${query}`;

  let res = http.get(url);

  check(res, {
    'status 200': (r) => r.status === 200,
    'tiempo < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}