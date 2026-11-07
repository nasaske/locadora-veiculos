// Endereço da API C#.
// - Rodando na máquina (localhost): API local, perfis "http" ou "demo" do launchSettings.json.
// - Versão online (Vercel): API publicada no Render, em modo demonstração.
// Também pode ser trocado sem editar este arquivo: API_URL=http://... npm start
const API_LOCAL = 'http://localhost:5147';
const API_ONLINE = 'https://locadora-veiculos-api.onrender.com';

const rodandoLocal = ['localhost', '127.0.0.1', '[::1]'].includes(window.location.hostname);

export const API_URL = rodandoLocal ? API_LOCAL : API_ONLINE;
