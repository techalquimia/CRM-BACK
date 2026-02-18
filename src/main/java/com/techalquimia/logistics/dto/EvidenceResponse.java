package com.techalquimia.logistics.dto;

import lombok.Builder;
import lombok.Data;

import java.time.Instant;
import java.util.UUID;

@Data
@Builder
public class EvidenceResponse {

    private UUID id;
    private String imageUrl;
    private Instant dateTime;
    private Double latitude;
    private Double longitude;
    private String evidenceType;
    private Boolean isSynced;
    private Instant createdAt;

    // Campos específicos para tickets
    private Double pesoTotal;
    private Double tara;
    private Double pesoNeto;
}
