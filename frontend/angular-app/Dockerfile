# Multi-stage Dockerfile for Angular frontend.
# Stage 1: Build Angular app with ng build --configuration=production
# Stage 2: Serve with nginx (SPA fallback)
#
# Build args allow overriding API URLs at build time (sed-replace in source).
# Usage: docker build --build-arg API_REST_URL=http://1.2.3.4:5000 ...

FROM node:22-alpine AS build

ARG API_REST_URL=http://localhost:5000
ARG API_GRPC_ENVOY_URL=http://localhost:8080
ARG API_GRPC_DIRECT_URL=http://localhost:5002
ARG RUNNER_URL=http://localhost:3100

WORKDIR /app

# Copy package files first for layer caching
COPY package.json package-lock.json ./
RUN npm ci

# Copy source
COPY . .

# Replace hardcoded localhost URLs with build-time values
RUN find src -name '*.ts' -exec sed -i \
    "s|http://localhost:5000|${API_REST_URL}|g" {} + && \
    find src -name '*.ts' -exec sed -i \
    "s|http://localhost:8080|${API_GRPC_ENVOY_URL}|g" {} + && \
    find src -name '*.ts' -exec sed -i \
    "s|http://localhost:5002|${API_GRPC_DIRECT_URL}|g" {} + && \
    find src -name '*.ts' -exec sed -i \
    "s|http://localhost:3100|${RUNNER_URL}|g" {} +

# Build production bundle
RUN npx ng build --configuration=production

# Stage 2: Serve with nginx
FROM nginx:alpine AS runtime

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Add custom nginx config with SPA fallback
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built Angular app (Angular 20 outputs to dist/<project-name>/browser/)
COPY --from=build /app/dist/angular-app/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
