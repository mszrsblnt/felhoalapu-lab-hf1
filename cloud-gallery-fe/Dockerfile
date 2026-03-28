FROM node:20-slim AS build
WORKDIR /app
COPY package*.json ./
RUN apt-get update && apt-get upgrade -y && npm install && apt-get clean && rm -rf /var/lib/apt/lists/*
COPY . .
RUN npx ng build --configuration production

FROM nginx:1.25.4-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/cloud-gallery-fe/browser /usr/share/nginx/html
EXPOSE 80

CMD ["/bin/sh", "-c", "sed -i 's|__API_URL__|'\"$API_URL\"'|g' /usr/share/nginx/html/*.js && nginx -g 'daemon off;'"]