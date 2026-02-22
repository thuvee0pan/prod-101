FROM node:20-alpine AS builder
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install

# Accept build args so Next.js compiles with correct values
ARG API_URL=http://api:5000
ARG NEXT_PUBLIC_GOOGLE_CLIENT_ID
ENV API_URL=${API_URL}
ENV NEXT_PUBLIC_GOOGLE_CLIENT_ID=${NEXT_PUBLIC_GOOGLE_CLIENT_ID}

COPY . .
# Remove local .env to prevent it from overriding the build arg
RUN rm -f .env .env.local
RUN npm run build

FROM node:20-alpine
WORKDIR /app
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./
COPY --from=builder /app/public ./public
COPY --from=builder /app/next.config.js ./

# Runtime env — used by Next.js server for rewrites
ENV API_URL=http://api:5000

EXPOSE 3000
CMD ["npm", "start"]
