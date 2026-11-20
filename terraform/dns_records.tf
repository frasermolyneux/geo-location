removed {
  from = azurerm_dns_txt_record.web_verification

  lifecycle {
    destroy = false
  }
}

removed {
  from = azurerm_dns_cname_record.web

  lifecycle {
    destroy = false
  }
}

import {
  for_each = var.environment == "prd" ? toset(["prd"]) : toset([])
  to       = cloudflare_dns_record.web[each.value]
  id       = "${var.dns.cloudflare_zone_id}/75ecf7ee68d9f0c2112c0c7244e3b6fd"
}

resource "cloudflare_dns_record" "web_verification" {
  zone_id = var.dns.cloudflare_zone_id
  name    = "asuid.${var.dns.web_subdomain}.${var.dns.domain}"
  type    = "TXT"
  ttl     = 300
  content = azurerm_linux_web_app.web.custom_domain_verification_id
}

resource "cloudflare_dns_record" "web" {
  for_each = toset([var.environment])

  zone_id = var.dns.cloudflare_zone_id
  name    = "${var.dns.web_subdomain}.${var.dns.domain}"
  type    = "CNAME"
  ttl     = var.environment == "prd" ? 1 : 300
  content = "${azurerm_linux_web_app.web.name}.azurewebsites.net"
  proxied = var.environment == "prd"

  depends_on = [cloudflare_dns_record.web_verification]
}
