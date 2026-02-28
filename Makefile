.PHONY: up down logs ps config rebuild

up:
	docker compose up --build -d

down:
	docker compose down

logs:
	docker compose logs -f --tail=200

ps:
	docker compose ps

config:
	docker compose config

rebuild:
	docker compose build --no-cache
