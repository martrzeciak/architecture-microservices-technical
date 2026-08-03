# Dockerfile for k6 Runner Server (Express.js).
# Runs inside Docker Compose and spawns k6 containers via Docker socket.
#
# Requires volume mounts:
#   - /var/run/docker.sock:/var/run/docker.sock (to spawn k6 containers)
#   - ./tests/k6:/app/k6-scripts (test scripts)
#   - ./tests/k6/results:/app/k6-results (output)

FROM node:22-alpine

# Install Docker CLI (needed to spawn k6 containers)
RUN apk add --no-cache docker-cli

WORKDIR /app

COPY package.json package-lock*.json ./
RUN npm ci --production

COPY server.js server-docker.js ./

ENV PORT=3100
EXPOSE 3100

# Use Docker-aware version (Linux paths, service names for health checks)
CMD ["node", "server-docker.js"]
