import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10,
  duration: '20s',
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<800'],
  },
};

const BASE_URL = __ENV.API_BASE_URL || 'http://localhost:8000';

export default function () {
  const params = {
    headers: {
      'Accept-Encoding': 'br, gzip',
    },
  };

  const health = http.get(`${BASE_URL}/health/ready`, params);
  check(health, { 'health 200': (r) => r.status === 200 });

  const rutas = http.get(`${BASE_URL}/api/v1/rutas`, params);
  check(rutas, { 'rutas 200': (r) => r.status === 200 });

  const cercanas = http.get(`${BASE_URL}/api/v1/paradas/cercanas?lat=20.0512&lon=-99.3421&radioKm=10`, params);
  check(cercanas, { 'paradas cercanas 200': (r) => r.status === 200 });

  const destinos = http.get(`${BASE_URL}/api/v1/destinos`, params);
  check(destinos, { 'destinos 200': (r) => r.status === 200 });

  sleep(0.5);
}
