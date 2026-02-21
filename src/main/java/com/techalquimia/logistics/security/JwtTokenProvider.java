package com.techalquimia.logistics.security;

import io.jsonwebtoken.Claims;
import io.jsonwebtoken.ExpiredJwtException;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.MalformedJwtException;
import io.jsonwebtoken.UnsupportedJwtException;
import io.jsonwebtoken.security.Keys;
import io.jsonwebtoken.security.SignatureException;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.util.Date;
import java.util.UUID;

@Component
@Slf4j
public class JwtTokenProvider {

    @Value("${jwt.secret}")
    private String jwtSecret;

    @Value("${jwt.expiration-ms:86400000}")
    private long jwtExpirationMs;

    private static final String CLAIM_USER_ID = "userId";

    private SecretKey getSigningKey() {
        try {
            byte[] keyBytes = jwtSecret.getBytes(StandardCharsets.UTF_8);
            if (keyBytes.length < 32) {
                keyBytes = java.security.MessageDigest.getInstance("SHA-256")
                        .digest(jwtSecret.getBytes(StandardCharsets.UTF_8));
            }
            return Keys.hmacShaKeyFor(keyBytes);
        } catch (java.security.NoSuchAlgorithmException e) {
            throw new RuntimeException("Algoritmo SHA-256 no disponible", e);
        }
    }

    public String generateToken(LoggedUser user) {
        try {
            Date now = new Date();
            Date expiryDate = new Date(now.getTime() + jwtExpirationMs);

            return Jwts.builder()
                    .subject(user.getUsername())
                    .claim(CLAIM_USER_ID, user.getId().toString())
                    .issuedAt(now)
                    .expiration(expiryDate)
                    .signWith(getSigningKey())
                    .compact();
        } catch (Exception e) {
            log.error("Error generando token JWT", e);
            throw new RuntimeException("Error generando token JWT", e);
        }
    }

    public UUID getUserIdFromToken(String token) {
        Claims claims = Jwts.parser()
                .verifyWith(getSigningKey())
                .build()
                .parseSignedClaims(token)
                .getPayload();

        String userId = claims.get(CLAIM_USER_ID, String.class);
        return userId != null ? UUID.fromString(userId) : null;
    }

    public boolean validateToken(String token) {
        try {
            Jwts.parser()
                    .verifyWith(getSigningKey())
                    .build()
                    .parseSignedClaims(token);
            return true;
        } catch (SignatureException ex) {
            log.warn("Firma JWT inválida");
        } catch (MalformedJwtException ex) {
            log.warn("Token JWT mal formado");
        } catch (ExpiredJwtException ex) {
            log.warn("Token JWT expirado");
        } catch (UnsupportedJwtException ex) {
            log.warn("Token JWT no soportado");
        } catch (IllegalArgumentException ex) {
            log.warn("JWT claims vacío");
        }
        return false;
    }
}
