package com.techalquimia.logistics.controller;

import com.techalquimia.logistics.dto.AuthRequest;
import com.techalquimia.logistics.dto.AuthResponse;
import com.techalquimia.logistics.security.JwtTokenProvider;
import com.techalquimia.logistics.security.LoggedUser;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/v1/auth")
@RequiredArgsConstructor
@Tag(name = "Autenticación", description = "Login y obtención de token JWT")
public class AuthController {

    private final AuthenticationManager authenticationManager;
    private final JwtTokenProvider tokenProvider;

    @PostMapping("/login")
    @Operation(summary = "Iniciar sesión", description = "Autentica con email y contraseña, devuelve un token JWT. Usar el token en header: Authorization: Bearer <token>")
    @ApiResponse(responseCode = "200", description = "Login exitoso, devuelve token JWT")
    @ApiResponse(responseCode = "401", description = "Credenciales inválidas")
    public ResponseEntity<AuthResponse> login(@Valid @RequestBody AuthRequest request) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(request.getEmail(), request.getPassword()));

        LoggedUser user = (LoggedUser) authentication.getPrincipal();
        String token = tokenProvider.generateToken(user);

        var response = AuthResponse.builder()
                .token(token)
                .type("Bearer")
                .userId(user.getId())
                .email(user.getEmail())
                .name(user.getName())
                .build();

        return ResponseEntity.ok(response);
    }
}
