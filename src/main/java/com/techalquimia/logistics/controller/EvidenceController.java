package com.techalquimia.logistics.controller;

import com.techalquimia.logistics.dto.EvidenceResponse;
import com.techalquimia.logistics.security.LoggedUser;
import com.techalquimia.logistics.service.EvidenceService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.Parameter;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/evidences")
@RequiredArgsConstructor
@Tag(name = "Evidencias", description = "Envío y gestión de evidencias de viaje (tickets y otros)")
public class EvidenceController {

    private final EvidenceService evidenceService;

    @PostMapping(consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    @ResponseStatus(HttpStatus.CREATED)
    @Operation(summary = "Crear evidencia", description = "Sube una evidencia (imagen). WEIGHT_TICKET extrae peso total, tara y peso neto por OCR.")
    @ApiResponse(responseCode = "201", description = "Evidencia creada correctamente")
    @ApiResponse(responseCode = "401", description = "No autenticado")
    @ApiResponse(responseCode = "413", description = "Archivo excede tamaño máximo (10MB)")
    public EvidenceResponse createEvidence(
            @AuthenticationPrincipal LoggedUser user,
            @RequestParam("image") MultipartFile image,
            @RequestParam("dateTime") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Instant dateTime,
            @RequestParam(value = "latitude", required = false) Double latitude,
            @RequestParam(value = "longitude", required = false) Double longitude,
            @Parameter(description = "Código del tipo: WEIGHT_TICKET, TRUCK_PHOTO, SEAL_PHOTO, DELIVERY_PROOF, INCIDENT_REPORT")
            @RequestParam("evidenceType") String evidenceType) throws Exception {
        return evidenceService.createEvidence(user.getId(), image, dateTime, latitude, longitude, evidenceType);
    }

    @GetMapping
    @Operation(summary = "Listar evidencias", description = "Lista las evidencias del usuario. Filtra por tipo si se especifica evidenceType.")
    @ApiResponse(responseCode = "200", description = "Lista de evidencias")
    public List<EvidenceResponse> getEvidences(
            @AuthenticationPrincipal LoggedUser user,
            @RequestParam(value = "evidenceType", required = false) String evidenceType) {
        if (evidenceType != null && !evidenceType.isBlank()) {
            return evidenceService.getEvidencesByUserAndType(user.getId(), evidenceType);
        }
        return evidenceService.getEvidencesByUser(user.getId());
    }

    @GetMapping("/{id}")
    @Operation(summary = "Obtener evidencia por ID")
    @ApiResponse(responseCode = "200", description = "Evidencia encontrada")
    @ApiResponse(responseCode = "404", description = "Evidencia no encontrada")
    public EvidenceResponse getEvidenceById(
            @AuthenticationPrincipal LoggedUser user,
            @PathVariable UUID id) {
        return evidenceService.getById(id, user.getId());
    }

    @PatchMapping("/{id}/sync")
    @Operation(summary = "Marcar como sincronizada")
    @ApiResponse(responseCode = "200", description = "Evidencia marcada como sincronizada")
    public EvidenceResponse markAsSynced(
            @AuthenticationPrincipal LoggedUser user,
            @PathVariable UUID id) {
        return evidenceService.markAsSynced(id, user.getId());
    }
}
