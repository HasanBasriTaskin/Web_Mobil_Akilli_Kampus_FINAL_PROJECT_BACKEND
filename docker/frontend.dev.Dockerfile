# Development Dockerfile for Next.js Frontend
# Uses development server with hot reload and runtime environment variables

FROM node:20-alpine

WORKDIR /app

# Install global dependencies
RUN npm install -g concurrently

# Install dependencies
COPY package*.json ./
RUN npm ci

# Copy source code (volume mount will override this in dev)
COPY . .

# Note: .env.local is loaded via volume mount in docker-compose.yml
# No need to COPY it here (it's also in .dockerignore)

EXPOSE 3000

# Run development server
CMD ["npm", "run", "dev"]

