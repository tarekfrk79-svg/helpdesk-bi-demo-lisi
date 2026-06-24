#!/usr/bin/env bash
set -euo pipefail

SITE_URL="${1:-}"
OWNER_CODE="${2:-}"
COMPANY_CODE="${3:-}"

if [[ -z "$SITE_URL" || -z "$OWNER_CODE" || -z "$COMPANY_CODE" ]]; then
  echo "Usage: bash scripts/check-live-flows.sh <site-url> <owner-code> <company-code>" >&2
  exit 1
fi

FORM_HTML="/tmp/helpdesk_form.html"
FORM_HTML_2="/tmp/helpdesk_form2.html"
COOKIES_1="/tmp/helpdesk_cookies.txt"
COOKIES_2="/tmp/helpdesk_cookies2.txt"
OWNER_POST_HEADERS="/tmp/helpdesk_owner_headers.txt"
OWNER_GET_HEADERS="/tmp/helpdesk_owner_get_headers.txt"
OWNER_BODY="/tmp/helpdesk_owner.html"
COMPANY_POST_HEADERS="/tmp/helpdesk_company_headers.txt"
COMPANY_GET_HEADERS="/tmp/helpdesk_company_get_headers.txt"
COMPANY_BODY="/tmp/helpdesk_company.html"

rm -f \
  "$FORM_HTML" "$FORM_HTML_2" \
  "$COOKIES_1" "$COOKIES_2" \
  "$OWNER_POST_HEADERS" "$OWNER_GET_HEADERS" "$OWNER_BODY" \
  "$COMPANY_POST_HEADERS" "$COMPANY_GET_HEADERS" "$COMPANY_BODY"

extract_token() {
  local file_path="$1"
  grep -oP 'name="__RequestVerificationToken" type="hidden" value="\K[^"]+' "$file_path" | head -n 1
}

curl -sS -c "$COOKIES_1" "$SITE_URL" -o "$FORM_HTML"
OWNER_TOKEN="$(extract_token "$FORM_HTML")"

curl -sS -b "$COOKIES_1" -c "$COOKIES_1" \
  -D "$OWNER_POST_HEADERS" \
  -o /tmp/helpdesk_owner_post.html \
  -X POST "$SITE_URL/Home/EnterCode" \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode "__RequestVerificationToken=$OWNER_TOKEN" \
  --data-urlencode "AccessCode=$OWNER_CODE"

curl -sS -b "$COOKIES_1" \
  -D "$OWNER_GET_HEADERS" \
  -o "$OWNER_BODY" \
  "$SITE_URL/Owner"

printf 'OWNER\n'
sed -n '1,12p' "$OWNER_POST_HEADERS"
sed -n '1,12p' "$OWNER_GET_HEADERS"
printf '\n---OWNER BODY---\n'
rg -n 'Espace Owner|Creer une entreprise|CONTOSO-DEMO|Contoso Support Demo' "$OWNER_BODY"

curl -sS -c "$COOKIES_2" "$SITE_URL" -o "$FORM_HTML_2"
COMPANY_TOKEN="$(extract_token "$FORM_HTML_2")"

curl -sS -b "$COOKIES_2" -c "$COOKIES_2" \
  -D "$COMPANY_POST_HEADERS" \
  -o /tmp/helpdesk_company_post.html \
  -X POST "$SITE_URL/Home/EnterCode" \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode "__RequestVerificationToken=$COMPANY_TOKEN" \
  --data-urlencode "AccessCode=$COMPANY_CODE"

curl -sS -b "$COOKIES_2" \
  -D "$COMPANY_GET_HEADERS" \
  -o "$COMPANY_BODY" \
  "$SITE_URL/Demo/RoleSelection"

printf '\nCOMPANY\n'
sed -n '1,12p' "$COMPANY_POST_HEADERS"
sed -n '1,12p' "$COMPANY_GET_HEADERS"
printf '\n---COMPANY BODY---\n'
rg -n 'Choisir un role|Contoso Support Demo|Administrateur|Technicien|Utilisateur' "$COMPANY_BODY"
