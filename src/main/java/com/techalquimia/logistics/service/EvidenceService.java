package com.techalquimia.logistics.service;

import com.techalquimia.logistics.dto.EvidenceResponse;
import com.techalquimia.logistics.entity.Evidence;
import com.techalquimia.logistics.entity.TicketWeight;
import com.techalquimia.logistics.repository.EvidenceRepository;
import com.techalquimia.logistics.service.TicketExtractionService.TicketData;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.time.Instant;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class EvidenceService {

    private static final String EVIDENCE_TYPE_TICKET = "TICKET";

    private final EvidenceRepository evidenceRepository;
    private final GcpStorageService gcpStorageService;
    private final TicketExtractionService ticketExtractionService;

    @Transactional
    public EvidenceResponse createEvidence(UUID userId, MultipartFile image, Instant dateTime,
                                          Double latitude, Double longitude, String evidenceType) throws IOException {
        String imageUrl = gcpStorageService.uploadImage(image, "evidences", userId);

        Evidence evidence = Evidence.builder()
                .imageUrl(imageUrl)
                .dateTime(dateTime)
                .latitude(latitude)
                .longitude(longitude)
                .evidenceType(evidenceType)
                .isSynced(false)
                .userId(userId)
                .build();

        if (EVIDENCE_TYPE_TICKET.equalsIgnoreCase(evidenceType)) {
            TicketData ticketData = ticketExtractionService.extractFromImage(image);
            TicketWeight ticketWeight = TicketWeight.builder()
                    .pesoTotal(ticketData.pesoTotal())
                    .tara(ticketData.tara())
                    .pesoNeto(ticketData.pesoNeto())
                    .evidence(evidence)
                    .build();
            evidence.setTicketWeight(ticketWeight);
        }

        evidence = evidenceRepository.save(evidence);
        return mapToResponse(evidence);
    }

    @Transactional(readOnly = true)
    public List<EvidenceResponse> getEvidencesByUser(UUID userId) {
        return evidenceRepository.findByUserIdOrderByCreatedAtDesc(userId)
                .stream()
                .map(this::mapToResponse)
                .collect(Collectors.toList());
    }

    @Transactional(readOnly = true)
    public List<EvidenceResponse> getEvidencesByUserAndType(UUID userId, String evidenceType) {
        return evidenceRepository.findByUserIdAndEvidenceTypeOrderByCreatedAtDesc(userId, evidenceType)
                .stream()
                .map(this::mapToResponse)
                .collect(Collectors.toList());
    }

    @Transactional(readOnly = true)
    public EvidenceResponse getById(UUID id, UUID userId) {
        Evidence evidence = evidenceRepository.findById(id).orElseThrow();
        if (!evidence.getUserId().equals(userId)) {
            throw new IllegalArgumentException("Evidencia no encontrada");
        }
        return mapToResponse(evidence);
    }

    @Transactional
    public EvidenceResponse markAsSynced(UUID id, UUID userId) {
        Evidence evidence = evidenceRepository.findById(id).orElseThrow();
        if (!evidence.getUserId().equals(userId)) {
            throw new IllegalArgumentException("Evidencia no encontrada");
        }
        evidence.setIsSynced(true);
        return mapToResponse(evidenceRepository.save(evidence));
    }

    private EvidenceResponse mapToResponse(Evidence e) {
        EvidenceResponse.EvidenceResponseBuilder builder = EvidenceResponse.builder()
                .id(e.getId())
                .imageUrl(e.getImageUrl())
                .dateTime(e.getDateTime())
                .latitude(e.getLatitude())
                .longitude(e.getLongitude())
                .evidenceType(e.getEvidenceType())
                .isSynced(e.getIsSynced())
                .createdAt(e.getCreatedAt());

        if (e.getTicketWeight() != null) {
            TicketWeight tw = e.getTicketWeight();
            builder.pesoTotal(tw.getPesoTotal()).tara(tw.getTara()).pesoNeto(tw.getPesoNeto());
        }
        return builder.build();
    }
}
