package com.techalquimia.logistics.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Builder;
import lombok.Data;

import java.util.UUID;

@Data
@Builder
@Schema(description = "Respuesta de usuario registrado")
public class UserResponse {

    @Schema(description = "ID del usuario")
    private UUID id;
    @Schema(description = "Email")
    private String email;
    @Schema(description = "Nombre")
    private String name;
    @Schema(description = "Fecha de creación")
    private String createdAt;
}
