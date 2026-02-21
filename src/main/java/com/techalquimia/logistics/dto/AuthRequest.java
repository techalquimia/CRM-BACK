package com.techalquimia.logistics.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
@Schema(description = "Credenciales de login")
public class AuthRequest {

    @Schema(description = "Email del usuario", example = "usuario@ejemplo.com", requiredMode = Schema.RequiredMode.REQUIRED)
    @NotBlank(message = "El email es obligatorio")
    private String email;

    @Schema(description = "Contraseña", requiredMode = Schema.RequiredMode.REQUIRED)
    @NotBlank(message = "La contraseña es obligatoria")
    private String password;
}
