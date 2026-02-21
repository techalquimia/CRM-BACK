package com.techalquimia.logistics.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Builder;
import lombok.Data;

import java.util.UUID;

@Data
@Builder
@Schema(description = "Respuesta de login con token JWT")
public class AuthResponse {

    @Schema(description = "Token JWT")
    private String token;

    @Schema(description = "Tipo de token")
    private String type;

    @Schema(description = "ID del usuario")
    private UUID userId;

    @Schema(description = "Email del usuario")
    private String email;

    @Schema(description = "Nombre del usuario")
    private String name;
}
