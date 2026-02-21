package com.techalquimia.logistics.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Builder;
import lombok.Data;

import java.util.UUID;

@Data
@Builder
@Schema(description = "Tipo de evidencia del catálogo")
public class EvidenceTypeCatalogResponse {

    @Schema(description = "ID del tipo")
    private UUID id;
    @Schema(description = "Código: WEIGHT_TICKET, TRUCK_PHOTO, SEAL_PHOTO, DELIVERY_PROOF, INCIDENT_REPORT")
    private String code;
    @Schema(description = "Nombre descriptivo")
    private String name;
    @Schema(description = "Descripción")
    private String description;
    @Schema(description = "Indica si requiere OCR (extracción de pesos)")
    private Boolean requiresOcr;
}
