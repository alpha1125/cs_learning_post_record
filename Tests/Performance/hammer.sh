#!/bin/bash

# Function to generate a random query parameter string of about 1k in size
generate_random_query_string() {
  local chars="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
  local query=""
  for i in {1..1000}; do
    query+="${chars:RANDOM%${#chars}:1}"
  done
  echo "$query"
}

# Save the URL to a file for siege
#echo "https://127.0.0.1:8001/ping?$(generate_random_query_string)" > urls.txt
#echo "https://staples-ca-pre-prox-app-dev2.lloydleung.com/ping?$(generate_random_query_string)" > urls.txt


### C#/.NET no https
# echo "http://127.0.0.1:5197/api/ping?$(generate_random_query_string)" > urls.txt


### C#/.NET https
echo "https://127.0.0.1:5001/api/ping?$(generate_random_query_string)" > urls.txt

# Run siege
siege -f urls.txt -c 50 -t 1m