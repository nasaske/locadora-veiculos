# Imagem do front-end para a versão online (Cloud Run).
# O endereço da API é informado na variável de ambiente API_URL.
FROM node:22-alpine
WORKDIR /app
COPY package.json server.js ./
COPY public ./public
ENV NODE_ENV=production \
    PORT=8080
EXPOSE 8080
USER node
CMD ["node", "server.js"]
