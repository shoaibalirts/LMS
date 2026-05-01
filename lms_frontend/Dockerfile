# Stage 1: Build the Vue app
FROM node:22-alpine AS build-stage
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm install

# Build the app for production
COPY . .
RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:alpine AS production-stage
# Copy the built files from the previous stage to Nginx's serving directory
COPY --from=build-stage /app/dist /usr/share/nginx/html

EXPOSE 80

# Nginx base image already has a built-in ENTRYPOINT/CMD to start the server, 
# so we don't strictly need to declare one here!