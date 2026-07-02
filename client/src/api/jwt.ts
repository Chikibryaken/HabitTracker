interface JwtPayload {
  sub: string;
  [claim: string]: unknown;
}

function base64UrlDecode(input: string): string {
  const base64 = input.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
  return atob(padded);
}

export function decodeJwtPayload(token: string): JwtPayload {
  const payload = token.split(".")[1];
  return JSON.parse(base64UrlDecode(payload)) as JwtPayload;
}
