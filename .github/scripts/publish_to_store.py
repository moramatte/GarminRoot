"""
Uploads a Connect IQ .iq package to the Garmin Connect IQ Store.

Usage:
    python3 publish_to_store.py <path/to/app.iq> <app-guid>

Authentication uses OAuth 1.0 (HMAC-SHA1). Set these environment variables:
    CIQ_CONSUMER_KEY
    CIQ_CONSUMER_SECRET
    CIQ_ACCESS_TOKEN
    CIQ_ACCESS_TOKEN_SECRET

Credentials are obtained from:
    https://developer.garmin.com/connect-iq/store-developer-resources/
"""

import base64
import hashlib
import hmac
import json
import os
import random
import string
import sys
import time
import urllib.parse
import urllib.request

STORE_API = "https://apps.garmin.com/api/appsLibraryExternalServices/api/asw"


def _percent_encode(value: str) -> str:
    return urllib.parse.quote(str(value), safe="")


def _oauth_header(method: str, url: str, extra_params: dict, secrets: dict) -> str:
    nonce = "".join(random.choices(string.ascii_letters + string.digits, k=32))
    timestamp = str(int(time.time()))

    oauth_params = {
        "oauth_consumer_key": secrets["consumer_key"],
        "oauth_nonce": nonce,
        "oauth_signature_method": "HMAC-SHA1",
        "oauth_timestamp": timestamp,
        "oauth_token": secrets["access_token"],
        "oauth_version": "1.0",
    }

    all_params = {**oauth_params, **extra_params}
    param_string = "&".join(
        f"{_percent_encode(k)}={_percent_encode(v)}"
        for k, v in sorted(all_params.items())
    )
    base_string = (
        f"{method.upper()}"
        f"&{_percent_encode(url)}"
        f"&{_percent_encode(param_string)}"
    )
    signing_key = (
        f"{_percent_encode(secrets['consumer_secret'])}"
        f"&{_percent_encode(secrets['access_token_secret'])}"
    )

    digest = hmac.new(
        signing_key.encode("ascii"),
        base_string.encode("ascii"),
        hashlib.sha1,
    ).digest()
    signature = base64.b64encode(digest).decode("ascii")
    oauth_params["oauth_signature"] = signature

    header_parts = ", ".join(
        f'{k}="{_percent_encode(v)}"' for k, v in sorted(oauth_params.items())
    )
    return f"OAuth {header_parts}"


def _multipart_body(fields: dict, files: dict):
    boundary = "----GarminUploadBoundary" + "".join(
        random.choices(string.ascii_letters + string.digits, k=16)
    )
    body_parts = []
    for name, value in fields.items():
        body_parts.append(
            f"--{boundary}\r\n"
            f'Content-Disposition: form-data; name="{name}"\r\n\r\n'
            f"{value}\r\n"
        )
    for name, (filename, data, content_type) in files.items():
        body_parts.append(
            f"--{boundary}\r\n"
            f'Content-Disposition: form-data; name="{name}"; filename="{filename}"\r\n'
            f"Content-Type: {content_type}\r\n\r\n"
        )
        body = "".join(body_parts).encode("utf-8") + data + f"\r\n".encode("utf-8")
        body_parts = []
        body += f"--{boundary}--\r\n".encode("utf-8")
        return body, boundary

    full = "".join(body_parts).encode("utf-8") + f"--{boundary}--\r\n".encode("utf-8")
    return full, boundary


def upload(iq_path: str, app_guid: str, secrets: dict) -> None:
    url = f"{STORE_API}/apps/{app_guid}/versions"
    print(f"Uploading {iq_path} → {url}")

    with open(iq_path, "rb") as f:
        iq_data = f.read()

    filename = os.path.basename(iq_path)
    body, boundary = _multipart_body(
        fields={},
        files={"iqFile": (filename, iq_data, "application/octet-stream")},
    )

    auth = _oauth_header("POST", url, {}, secrets)

    req = urllib.request.Request(
        url,
        data=body,
        method="POST",
        headers={
            "Authorization": auth,
            "Content-Type": f"multipart/form-data; boundary={boundary}",
        },
    )

    try:
        with urllib.request.urlopen(req) as resp:
            status = resp.status
            body_text = resp.read().decode("utf-8", errors="replace")
    except urllib.error.HTTPError as exc:
        status = exc.code
        body_text = exc.read().decode("utf-8", errors="replace")

    print(f"HTTP {status}")
    if body_text:
        try:
            print(json.dumps(json.loads(body_text), indent=2))
        except json.JSONDecodeError:
            print(body_text)

    if not (200 <= status < 300):
        sys.exit(f"Upload failed with HTTP {status}")

    print("Upload successful.")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit(f"Usage: {sys.argv[0]} <app.iq> <app-guid>")

    iq_file, guid = sys.argv[1], sys.argv[2]

    for var in ("CIQ_CONSUMER_KEY", "CIQ_CONSUMER_SECRET", "CIQ_ACCESS_TOKEN", "CIQ_ACCESS_TOKEN_SECRET"):
        if not os.environ.get(var):
            sys.exit(f"Missing required environment variable: {var}")

    upload(
        iq_path=iq_file,
        app_guid=guid,
        secrets={
            "consumer_key": os.environ["CIQ_CONSUMER_KEY"],
            "consumer_secret": os.environ["CIQ_CONSUMER_SECRET"],
            "access_token": os.environ["CIQ_ACCESS_TOKEN"],
            "access_token_secret": os.environ["CIQ_ACCESS_TOKEN_SECRET"],
        },
    )
