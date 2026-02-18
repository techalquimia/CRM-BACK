package com.techalquimia.logistics.controller;

import com.techalquimia.logistics.dto.EvidenceResponse;
import com.techalquimia.logistics.security.LoggedUser;
import com.techalquimia.logistics.service.EvidenceService;
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
public class EvidenceController {

    private final EvidenceService evidenceService;

    @PostMapping(consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    @ResponseStatus(HttpStatus.CREATED)
    public EvidenceResponse createEvidence(
            @AuthenticationPrincipal LoggedUser user,
            @RequestParam("image") MultipartFile image,
            @RequestParam("dateTime") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Instant dateTime,
            @RequestParam(value = "latitude", required = false) Double latitude,
            @RequestParam(value = "longitude", required = false) Double longitude,
            @RequestParam("evidenceType") String evidenceType) throws Exception {
        return evidenceService.createEvidence(user.getId(), image, dateTime, latitude, longitude, evidenceType);
    }

    @GetMapping
    public List<EvidenceResponse> getEvidences(
            @AuthenticationPrincipal LoggedUser user,
            @RequestParam(value = "evidenceType", required = false) String evidenceType) {
        if (evidenceType != null && !evidenceType.isBlank()) {
            return evidenceService.getEvidencesByUserAndType(user.getId(), evidenceType);
        }
        return evidenceService.getEvidencesByUser(user.getId());
    }

    @GetMapping("/{id}")
    public EvidenceResponse getEvidenceById(
            @AuthenticationPrincipal LoggedUser user,
            @PathVariable UUID id) {
        return evidenceService.getById(id, user.getId());
    }

    @PatchMapping("/{id}/sync")
    public EvidenceResponse markAsSynced(
            @AuthenticationPrincipal LoggedUser user,
            @PathVariable UUID id) {
        return evidenceService.markAsSynced(id, user.getId());
    }
}
