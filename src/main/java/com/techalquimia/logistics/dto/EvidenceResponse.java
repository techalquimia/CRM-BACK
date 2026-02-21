package com.techalquimia.logistics.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Builder;
import lombok.Data;

import java.time.Instant;
import java.util.UUID;

@Data
@Builder
@Schema(description = "Evidencia de viaje")
public class EvidenceResponse {

    @Schema(description = "ID de la evidencia")
    private UUID id;
    @Schema(description = "URL de la imagen (GCP o local)")
    private String imageUrl;
    @Schema(description = "Fecha y hora del registro")
    private Instant dateTime;
    @Schema(description = "Latitud")
    private Double latitude;
    @Schema(description = "Longitud")
    private Double longitude;
    @Schema(description = "Código del tipo: WEIGHT_TICKET, TRUCK_PHOTO, SEAL_PHOTO, DELIVERY_PROOF, INCIDENT_REPORT")
    private String evidenceType;
    @Schema(description = "Nombre descriptivo del tipo")
    private String evidenceTypeName;
    @Schema(description = "Indica si está sincronizada")
    private Boolean isSynced;
    @Schema(description = "Fecha de creación")
    private Instant createdAt;

    @Schema(description = "Peso total (solo tickets). peso_total = tara + peso_neto")
    private Double pesoTotal;
    @Schema(description = "Tara (solo tickets)")
    private Double tara;
    @Schema(description = "Peso neto (solo tickets)")
    private Double pesoNeto;
}
