call heroku container:login
docker build -t tvscreener .
call heroku container:push -a tvscreener web
heroku container:release -a tvscreener web