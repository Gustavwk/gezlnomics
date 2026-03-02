.PHONY: up down logs ps config rebuild up-prod down-prod backup restore

up:
	docker compose up --build -d

up-prod:
	docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up --build -d

down:
	docker compose down

down-prod:
	docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production down

logs:
	docker compose logs -f --tail=200

ps:
	docker compose ps

config:
	docker compose config

rebuild:
	docker compose build --no-cache

backup:
	./scripts/backup-postgres.sh

restore:
	./scripts/restore-postgres.sh $(FILE)
