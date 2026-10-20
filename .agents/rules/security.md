---
trigger: model_decision
description: Apply when handling authentication, authorization, secrets, personal data, external input, files, or infrastructure configuration.
---

# Security Rule

- Never hardcode credentials, API keys, private keys, or production secrets.
- Use the repository's established secret-management and configuration approach.
- Validate untrusted input at appropriate boundaries and enforce authorization on the server side.
- Avoid logging credentials, tokens, sensitive personal data, or full request bodies by default.
- Do not weaken authentication, authorization, TLS, CORS, or security checks merely to make a test pass.
- Flag security-sensitive design choices that cannot be resolved from the task context.
