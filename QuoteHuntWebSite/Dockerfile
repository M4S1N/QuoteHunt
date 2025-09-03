# Stage 0 - Run Tests
FROM node:20 as test
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .

RUN npm test -- --watch=false

# Stage 1 - Build Angular
FROM node:20 as build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build:prod

# Stage 2 - Serve with Nginx
FROM nginx:alpine

# Remove default nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy built Angular app to Nginx's public folder
COPY --from=build /app/dist/QuoteHuntWebSite/browser /usr/share/nginx/html

COPY default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
